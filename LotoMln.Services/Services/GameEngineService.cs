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

        var players = await uow.Players.GetByRoomCodeAsync(roomCode, ct);
        var gamers = players.Where(p => p.Id != room.HostId).ToList();

        if (gamers.Count < 2)
            throw new InvalidOperationException("Cần ít nhất 2 người chơi (không tính host)");

        var notPicked = gamers.Where(p => p.CardId == null).ToList();
        if (notPicked.Any())
            throw new InvalidOperationException(
                $"Các player chưa chọn card: {string.Join(", ", notPicked.Select(p => p.Name))}");

        // Lấy random questions (cho phép recycle nếu < 40)
        var normalQs = await uow.Questions.GetRandomByTypeAsync(
            QuestionType.Normal, GameConstants.NumberPoolSize * 2, ct);
        if (normalQs.Count == 0)
            throw new InvalidOperationException("DB chưa có câu hỏi nào (cần seed)");

        // Tạo 40 slots, recycle questions nếu < 40
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

        // Shuffle player queue
        var queue = gamers.Select(p => p.Id)
            .OrderBy(_ => Random.Shared.Next()).ToList();

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

        logger.LogInformation("Room {Code} game started with {N} players, {Q} unique questions",
            roomCode, players.Count, normalQs.Count);

        var stateDto = await BuildStateAsync(roomCode, ct);
        await notifier.GameStartedAsync(roomCode, stateDto);

        // Auto-trigger lượt đầu: chuyển Idle → DrawerSelecting, chọn drawer, broadcast TurnStarted
        return await SelectNextDrawerAsync(roomCode, ct);
    }

    public async Task<GameStateDto> SelectNextDrawerAsync(
    string roomCode, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);

        // Cho phép từ: Idle (đầu game), Revealing (giữa turn), DrawerSelecting (skip drawer timeout)
        if (state.Phase is not (GamePhase.Idle or GamePhase.Revealing or GamePhase.DrawerSelecting))
            throw new InvalidOperationException($"Phase {state.Phase} không cho phép chọn drawer");

        var nextDrawer = state.PlayerQueue[state.QueueIndex % state.PlayerQueue.Count];
        state.QueueIndex = (state.QueueIndex + 1) % state.PlayerQueue.Count;
        state.CurrentDrawerId = nextDrawer;
        state.CurrentSlotId = null;
        state.Phase = GamePhase.DrawerSelecting;
        state.PhaseStartedAt = DateTime.UtcNow;
        state.Deadline = DateTime.UtcNow.AddSeconds(15);

        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var stateDto = await BuildStateAsync(roomCode, ct);
        await notifier.TurnStartedAsync(roomCode, nextDrawer, state.Deadline!.Value);
        return stateDto;
    }

    public async Task<DrawerPickResponse> OnDrawerPicksSlotAsync(
        string roomCode, DrawerPickRequest req, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.DrawerSelecting)
            throw new InvalidOperationException("Không phải phase chọn slot");
        if (state.CurrentDrawerId != req.PlayerId)
            throw new InvalidOperationException("Bạn không phải drawer hiện tại");
        if (req.Position < 1 || req.Position > GameConstants.NumberPoolSize)
            throw new InvalidOperationException("Position phải từ 1-40");

        var slot = await uow.QuestionSlots.GetByPositionAsync(roomCode, req.Position, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");
        if (slot.Status != SlotStatus.Available)
            throw new InvalidOperationException("Slot đã bị mở/khóa");

        var question = await uow.Questions.GetByIdAsync(slot.QuestionId, ct)
            ?? throw new InvalidOperationException("Câu hỏi không tồn tại");

        state.CurrentSlotId = slot.Id;
        state.Phase = GamePhase.DrawerAnswering;
        state.PhaseStartedAt = DateTime.UtcNow;
        state.Deadline = DateTime.UtcNow.AddSeconds(GameConstants.TurnDurationSec);

        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var questionDto = new QuestionDto(question.Id, question.Text, question.Options, question.Type);
        await notifier.QuestionShownAsync(                                    // ← thêm
            roomCode, req.PlayerId, questionDto, slot.AssignedNumber, state.Deadline!.Value);

        return new DrawerPickResponse(questionDto, slot.AssignedNumber, state.Deadline.Value);
    }

    public async Task<SubmitAnswerResponse> OnDrawerAnswersAsync(
        string roomCode, SubmitAnswerRequest req, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.DrawerAnswering)
            throw new InvalidOperationException("Không phải phase drawer trả lời");
        if (state.CurrentDrawerId != req.PlayerId)
            throw new InvalidOperationException("Bạn không phải drawer");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Chưa pick slot");

        var slot = await uow.QuestionSlots.GetByIdWithQuestionAsync(state.CurrentSlotId.Value, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");

        bool isCorrect = req.AnswerIndex == slot.Question.CorrectIndex;
        int correctIdx = slot.Question.CorrectIndex;
        int? calledNum = null;
        GamePhase nextPhase;

        if (isCorrect)
        {
            // ĐÚNG → mark slot answered + add called number
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

            calledNum = slot.AssignedNumber;
            nextPhase = GamePhase.Revealing;
            state.Phase = GamePhase.Revealing;
            state.Deadline = DateTime.UtcNow.AddSeconds(5);
        }
        else
        {
            // SAI → vào steal mode
            nextPhase = GamePhase.Stealing;
            state.Phase = GamePhase.Stealing;
            state.Deadline = DateTime.UtcNow.AddSeconds(GameConstants.StealTimeoutSec);
        }

        state.PhaseStartedAt = DateTime.UtcNow;
        uow.GameStates.Update(state);
        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        await notifier.AnswerSubmittedAsync(roomCode, req.PlayerId, isCorrect, correctIdx);  // ← thêm

        if (isCorrect && calledNum.HasValue)
            await notifier.NumberCalledAsync(roomCode, calledNum.Value, req.PlayerId);       // ← thêm
        else if (!isCorrect)
            await notifier.StealModeStartedAsync(roomCode, state.Deadline!.Value);           // ← thêm

        return new SubmitAnswerResponse(isCorrect, correctIdx, nextPhase, calledNum);
    }

    public async Task<StealAttemptResponse> RecordStealAttemptAsync(
        string roomCode, StealAttemptRequest req, CancellationToken ct = default)
    {
        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.Stealing)
            throw new InvalidOperationException("Không phải phase steal");
        if (state.CurrentDrawerId == req.PlayerId)
            throw new InvalidOperationException("Drawer không được steal câu mình");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Không có slot active");

        var slot = await uow.QuestionSlots.GetByIdWithQuestionAsync(state.CurrentSlotId.Value, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");

        // Một player chỉ steal 1 lần / slot
        var existing = (await uow.StealAttempts.GetBySlotAsync(roomCode, slot.Id, ct))
            .FirstOrDefault(s => s.PlayerId == req.PlayerId);
        if (existing != null)
            throw new InvalidOperationException("Bạn đã steal câu này rồi");

        var attempt = new StealAttempt
        {
            Id = Guid.NewGuid(),
            RoomCode = roomCode,
            PlayerId = req.PlayerId,
            SlotId = slot.Id,
            Answer = req.AnswerIndex,
            Timestamp = DateTime.UtcNow,    // server time để fair race
            IsCorrect = req.AnswerIndex == slot.Question.CorrectIndex
        };
        await uow.StealAttempts.AddAsync(attempt, ct);
        await uow.SaveChangesAsync(ct);

        return new StealAttemptResponse(true, attempt.Timestamp);
    }

    public async Task<StealResolveResult> ResolveStealAsync(
        string roomCode, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var state = await uow.GameStates.GetRequiredAsync(roomCode, ct);
        if (state.Phase != GamePhase.Stealing)
            throw new InvalidOperationException("Không phải phase steal");
        if (state.CurrentSlotId == null)
            throw new InvalidOperationException("Không có slot active");

        var slot = await uow.QuestionSlots.GetByIdAsync(state.CurrentSlotId.Value, ct)
            ?? throw new InvalidOperationException("Slot không tồn tại");

        // Lấy attempt CORRECT sớm nhất (fair race theo timestamp)
        var winner = await uow.StealAttempts.GetFirstCorrectAsync(roomCode, slot.Id, ct);

        Guid? winnerId = null;
        int? calledNum = null;
        bool locked = false;

        if (winner != null)
        {
            slot.Status = SlotStatus.Answered;
            slot.AnsweredByPlayerId = winner.PlayerId;
            uow.QuestionSlots.Update(slot);

            await uow.CalledNumbers.AddAsync(new CalledNumber
            {
                RoomCode = roomCode,
                Number = slot.AssignedNumber,
                CalledAt = DateTime.UtcNow,
                CalledByPlayerId = winner.PlayerId
            }, ct);

            winnerId = winner.PlayerId;
            calledNum = slot.AssignedNumber;
        }
        else
        {
            // Không ai đúng → lock slot, số đó vĩnh viễn không được call
            slot.Status = SlotStatus.Locked;
            uow.QuestionSlots.Update(slot);
            locked = true;
        }

        state.Phase = GamePhase.Revealing;
        state.PhaseStartedAt = DateTime.UtcNow;
        state.Deadline = DateTime.UtcNow.AddSeconds(5);
        uow.GameStates.Update(state);

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Steal resolved in {Code}: winner={Winner}, locked={Locked}",
            roomCode, winnerId, locked);

        return new StealResolveResult(winnerId, calledNum, locked);
    }

    public async Task<bool> MarkNumberAsync(
        string roomCode, MarkNumberRequest req, CancellationToken ct = default)
    {
        var player = await uow.Players.GetByIdAsync(req.PlayerId, ct)
            ?? throw new InvalidOperationException("Player không tồn tại");
        if (player.RoomCode != roomCode)
            throw new InvalidOperationException("Player không thuộc phòng này");

        var isCalled = await uow.CalledNumbers.IsCalledAsync(roomCode, req.Number, ct);
        if (!isCalled)
            throw new InvalidOperationException($"Số {req.Number} chưa được gọi");

        if (!player.MarkedNumbers.Contains(req.Number))
        {
            player.MarkedNumbers = [.. player.MarkedNumbers, req.Number];
            uow.Players.Update(player);
            await uow.SaveChangesAsync(ct);
        }
        return true;
    }

    public async Task<KinhVerifyResult> ClaimKinhAsync(
    string roomCode, ClaimKinhRequest req, CancellationToken ct = default)
    {
        // ↓ Redis distributed lock: chỉ 1 player thắng race
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
            lockedPositions
        );
    }
}