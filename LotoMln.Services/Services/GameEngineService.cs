using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.DTOs;
using LotoMln.Models.Entities;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using LotoMln.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace LotoMln.Services.Services;

public class GameEngineService(
    IUnitOfWork uow,
    IKinhVerifierService kinhVerifier,
    IGameNotifier notifier,
    IGameCacheService cache,
    ILogger<GameEngineService> logger) : IGameEngineService
{
    public async Task<GameStateDto> StartGameAsync(
        string roomCode, Guid initiatorId, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException("Room không tồn tại");
        if (room.HostId != initiatorId)
            throw new InvalidOperationException("Chỉ host được start game");
        if (room.State != RoomState.Lobby)
            throw new InvalidOperationException("Game đã start hoặc kết thúc");

        var allPlayers = await uow.Players.GetByRoomCodeAsync(roomCode, ct);
        var gamers = allPlayers.Where(p => p.Id != room.HostId).ToList();

        if (gamers.Count < 2)
            throw new InvalidOperationException("Cần ít nhất 2 người chơi (không tính host)");

        var notPicked = gamers.Where(p => p.CardId == null).ToList();
        if (notPicked.Any())
            throw new InvalidOperationException(
                $"Các player chưa chọn card: {string.Join(", ", notPicked.Select(p => p.Name))}");

        var normalQs = await uow.Questions.GetRandomByTypeAsync(
            QuestionType.Normal, GameConstants.NumberPoolSize * 2, ct);
        if (normalQs.Count == 0)
            throw new InvalidOperationException("DB chưa có câu hỏi nào (cần seed)");

        var slots = new List<QuestionSlot>(GameConstants.NumberPoolSize);
        for (int i = 0; i < GameConstants.NumberPoolSize; i++)
        {
            slots.Add(new QuestionSlot
            {
                Id = Guid.NewGuid(),
                RoomCode = roomCode,
                Position = i + 1,
                QuestionId = normalQs[i % normalQs.Count].Id,
                AssignedNumber = i + 1,
                Status = SlotStatus.Available
            });
        }
        await uow.QuestionSlots.AddRangeAsync(slots, ct);

        var queue = gamers.Select(p => p.Id).ToList();

        var state = new GameStateSnapshot
        {
            RoomCode = roomCode,
            Phase = GamePhase.Idle,
            PlayerQueue = queue,
            QueueIndex = 0,
            PhaseStartedAt = DateTime.UtcNow
        };
        await uow.GameStates.AddAsync(state, ct);

        room.State = RoomState.Playing;
        uow.Rooms.Update(room);

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Room {Code} game started with {N} gamers, {Q} unique questions",
            roomCode, gamers.Count, normalQs.Count);

        var stateDto = await BuildStateAsync(roomCode, ct);
        await notifier.GameStartedAsync(roomCode, stateDto);
        return stateDto;
    }

    public async Task<SpinWheelResponse> SpinWheelAsync(
        string roomCode, Guid hostId, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException("Room không tồn tại");
        if (room.HostId != hostId)
            throw new InvalidOperationException("Chỉ host được quay số");

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.Idle)
            throw new InvalidOperationException("Chỉ được quay khi game đang ở trạng thái Idle");

        var slots = await uow.QuestionSlots.GetByRoomCodeAsync(roomCode, ct);
        var available = slots.Where(s => s.Status == SlotStatus.Available).ToList();
        if (available.Count == 0)
            throw new InvalidOperationException("Không còn câu hỏi — game kết thúc");

        var slot = available[Random.Shared.Next(available.Count)];
        var question = await uow.Questions.GetByIdAsync(slot.QuestionId, ct)
            ?? throw new InvalidOperationException("Câu hỏi không tồn tại");

        // ← New flow: KHÔNG auto assign drawer. Host sẽ chọn người trả lời.
        state.CurrentDrawerId = null;
        state.CurrentSlotId = slot.Id;
        state.Phase = GamePhase.DrawerAnswering;
        state.PhaseStartedAt = DateTime.UtcNow;
        state.Deadline = null;  // host control, không có timer

        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var questionDto = new QuestionDto(question.Id, question.Text, question.Options, question.Type);
        await notifier.WheelSpunAsync(roomCode, slot.AssignedNumber, questionDto);

        logger.LogInformation("Room {Code}: wheel spun → number {N} (host sẽ chọn người trả lời)",
            roomCode, slot.AssignedNumber);

        return new SpinWheelResponse(slot.AssignedNumber, questionDto);
    }

    public async Task SelectAnswererAsync(
        string roomCode, Guid hostId, Guid playerId, CancellationToken ct = default)
    {
        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException("Room không tồn tại");
        if (room.HostId != hostId)
            throw new InvalidOperationException("Chỉ host được chọn người trả lời");

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.DrawerAnswering)
            throw new InvalidOperationException("Chỉ chọn người trả lời được khi có câu hỏi active");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Chưa có slot active");

        if (!state.PlayerQueue.Contains(playerId))
            throw new InvalidOperationException("Player không trong danh sách chơi");

        state.CurrentDrawerId = playerId;
        state.PhaseStartedAt = DateTime.UtcNow;
        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);

        await notifier.AnswererSelectedAsync(roomCode, playerId);
        logger.LogInformation("Room {Code}: host chose {Pid} to answer", roomCode, playerId);
    }

    public async Task AdvanceToIdleAsync(string roomCode, CancellationToken ct = default)
    {
        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.Revealing) return;

        state.Phase = GamePhase.Idle;
        state.CurrentDrawerId = null;
        state.CurrentSlotId = null;
        state.Deadline = null;
        state.PhaseStartedAt = DateTime.UtcNow;

        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);

        await notifier.ReadyToSpinAsync(roomCode);
        logger.LogInformation("Room {Code}: Revealing ended → Idle (ready to spin)", roomCode);
    }

    public async Task<SubmitAnswerResponse> OnDrawerAnswersAsync(
        string roomCode, SubmitAnswerRequest req, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.DrawerAnswering)
            throw new InvalidOperationException("Không phải phase trả lời");
        if (state.CurrentDrawerId == null)
            throw new InvalidOperationException("Host chưa chọn người trả lời");
        if (state.CurrentDrawerId != req.PlayerId)
            throw new InvalidOperationException("Player này không phải drawer hiện tại");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Chưa có slot active");

        var slot = await uow.QuestionSlots.GetByIdWithQuestionAsync(state.CurrentSlotId.Value, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");

        bool isCorrect = req.AnswerIndex == slot.Question.CorrectIndex;
        int correctIdx = slot.Question.CorrectIndex;
        int? calledNum = null;
        GamePhase nextPhase;

        if (isCorrect)
        {
            slot.Status = SlotStatus.Answered;
            slot.AnsweredByPlayerId = req.PlayerId;
            uow.QuestionSlots.Update(slot);

            await uow.CalledNumbers.AddAsync(new CalledNumber
            {
                RoomCode = roomCode,
                Number = slot.AssignedNumber,
                CalledAt = DateTime.UtcNow,
                CalledByPlayerId = req.PlayerId
            }, ct);

            await AutoMarkForWinnerAsync(req.PlayerId, slot.AssignedNumber, ct);

            calledNum = slot.AssignedNumber;
            nextPhase = GamePhase.Revealing;
            state.Phase = GamePhase.Revealing;
            state.Deadline = DateTime.UtcNow.AddSeconds(5);
        }
        else
        {
            // SAI → clear drawer, host chọn người khác. Slot vẫn active.
            nextPhase = GamePhase.DrawerAnswering;
            state.Phase = GamePhase.DrawerAnswering;
            state.CurrentDrawerId = null;
            state.Deadline = null;
        }

        state.PhaseStartedAt = DateTime.UtcNow;
        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await notifier.AnswerSubmittedAsync(roomCode, req.PlayerId, isCorrect, correctIdx);

        if (isCorrect && calledNum.HasValue)
            await notifier.NumberCalledAsync(roomCode, calledNum.Value, req.PlayerId);

        return new SubmitAnswerResponse(isCorrect, correctIdx, nextPhase, calledNum);
    }

    public async Task SkipSlotAsync(string roomCode, Guid hostId, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException("Room không tồn tại");
        if (room.HostId != hostId)
            throw new InvalidOperationException("Chỉ host được bỏ qua slot");

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.DrawerAnswering)
            throw new InvalidOperationException("Chỉ bỏ qua được khi đang ở phase trả lời");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Không có slot active");

        var slot = await uow.QuestionSlots.GetByIdAsync(state.CurrentSlotId.Value, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");

        slot.Status = SlotStatus.Locked;
        uow.QuestionSlots.Update(slot);

        state.Phase = GamePhase.Revealing;
        state.CurrentDrawerId = null;
        state.Deadline = DateTime.UtcNow.AddSeconds(5);
        state.PhaseStartedAt = DateTime.UtcNow;
        uow.GameStates.Update(state);

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await notifier.SlotSkippedAsync(roomCode, slot.AssignedNumber);
        logger.LogInformation("Room {Code}: host skipped slot #{N}", roomCode, slot.AssignedNumber);
    }

    public async Task<KinhVerifyResult> ClaimKinhAsync(
        string roomCode, ClaimKinhRequest req, CancellationToken ct = default)
    {
        var acquired = await cache.TryAcquireKinhLockAsync(
            roomCode, req.PlayerId, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            var owner = await cache.GetKinhLockOwnerAsync(roomCode);
            var result = new KinhVerifyResult(false, WinType.Row, -1,
                $"Player {owner} đã claim trước (race lost)");
            await notifier.KinhClaimedAsync(roomCode, req.PlayerId, false, result.Reason);
            return result;
        }

        await using var tx = await uow.BeginTransactionAsync(ct);

        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException("Room không tồn tại");
        if (room.State != RoomState.Playing)
            throw new InvalidOperationException("Game chưa chạy hoặc đã kết thúc");

        var verifyResult = await kinhVerifier.VerifyAsync(roomCode, req.PlayerId, ct);

        await uow.KinhClaims.AddAsync(new KinhClaim
        {
            Id = Guid.NewGuid(),
            RoomCode = roomCode,
            PlayerId = req.PlayerId,
            ClaimedAt = DateTime.UtcNow,
            Verified = verifyResult.IsValid,
            WinType = verifyResult.WinType,
            WinIndex = verifyResult.WinIndex
        }, ct);

        if (verifyResult.IsValid)
        {
            room.State = RoomState.Ended;
            room.WinnerId = req.PlayerId;
            uow.Rooms.Update(room);
            logger.LogInformation("🏆 Player {Pid} won {Code}", req.PlayerId, roomCode);
        }

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await notifier.KinhClaimedAsync(roomCode, req.PlayerId, verifyResult.IsValid, verifyResult.Reason);
        if (verifyResult.IsValid)
            await notifier.GameEndedAsync(roomCode, req.PlayerId);

        return verifyResult;
    }

    public async Task<GameStateDto?> GetGameStateAsync(
        string roomCode, CancellationToken ct = default)
    {
        var state = await uow.GameStates.GetByRoomCodeAsync(roomCode, ct);
        if (state == null) return null;
        return await BuildStateAsync(roomCode, ct);
    }

    private async Task AutoMarkForWinnerAsync(Guid playerId, int number, CancellationToken ct)
    {
        var player = await uow.Players.GetWithCardAsync(playerId, ct);
        if (player?.Card == null) return;
        bool hasNumber = player.Card.Grid.Any(row => row.Contains(number));
        if (hasNumber && !player.MarkedNumbers.Contains(number))
        {
            player.MarkedNumbers = [.. player.MarkedNumbers, number];
            uow.Players.Update(player);
            await uow.SaveChangesAsync(ct);
        }
    }

    private async Task<GameStateDto> BuildStateAsync(string roomCode, CancellationToken ct)
    {
        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        var called = await uow.CalledNumbers.GetCalledNumbersAsync(roomCode, ct);
        var slots = await uow.QuestionSlots.GetByRoomCodeAsync(roomCode, ct);

        var answeredPositions = slots
            .Where(s => s.Status == SlotStatus.Answered)
            .Select(s => s.Position).OrderBy(p => p).ToList();
        var lockedPositions = slots
            .Where(s => s.Status == SlotStatus.Locked)
            .Select(s => s.Position).OrderBy(p => p).ToList();
        var remaining = slots.Count - answeredPositions.Count - lockedPositions.Count;

        int? spunNumber = null;
        if (state.CurrentSlotId.HasValue)
        {
            var currentSlot = slots.FirstOrDefault(s => s.Id == state.CurrentSlotId.Value);
            spunNumber = currentSlot?.AssignedNumber;
        }

        return new GameStateDto(
            roomCode,
            state.Phase,
            state.CurrentDrawerId,
            state.CurrentSlotId,
            state.Deadline,
            called,
            remaining,
            lockedPositions.Count,
            answeredPositions,
            lockedPositions,
            spunNumber
        );
    }
}
