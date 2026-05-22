using AutoMapper;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.DTOs;
using LotoMln.Models.Entities;
using LotoMln.Models.Enums;
using LotoMln.Services.Helpers;
using LotoMln.Services.IServices;
using LotoMln.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace LotoMln.Services.Services;

public class RoomService(
    IUnitOfWork uow,
    ICardGeneratorService cardGen,
    IMapper mapper,
    IGameNotifier notifier,
    ILogger<RoomService> logger) : IRoomService
{
    public async Task<CreateRoomResponse> CreateRoomAsync(
    CreateRoomRequest req, CancellationToken ct = default)
    {
        // Server tự sinh HostId
        var hostId = Guid.NewGuid();

        // Sinh room code không trùng
        string code;
        int attempts = 0;
        do
        {
            code = RoomCodeGenerator.Generate(GameConstants.RoomCodeLength);
            if (++attempts > 100)
                throw new InvalidOperationException("Không sinh được room code unique");
        } while (await uow.Rooms.ExistsAsync(code, ct));

        var room = new Room
        {
            Code = code,
            HostId = hostId,
            State = RoomState.Lobby,
            CreatedAt = DateTime.UtcNow,
            MaxPlayers = GameConstants.MaxPlayers,
            TurnDurationSec = GameConstants.TurnDurationSec,
            StealTimeoutSec = GameConstants.StealTimeoutSec,
            NameMaxLength = GameConstants.NameMaxLength
        };
        await uow.Rooms.AddAsync(room, ct);

        await uow.Players.AddAsync(new Player
        {
            Id = hostId,
            RoomCode = code,
            Name = req.HostName,
            JoinedAt = DateTime.UtcNow,
            Online = true
        }, ct);

        var cards = cardGen.GenerateCardsForRoom(code, GameConstants.TotalCards);
        await uow.Cards.AddRangeAsync(cards, ct);

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Room {Code} created by host {HostId} ({Name})",
            code, hostId, req.HostName);

        return new CreateRoomResponse(code, hostId, req.HostName);
    }

    public async Task<JoinRoomResponse> JoinRoomAsync(
    string roomCode, JoinRoomRequest req, CancellationToken ct = default)
    {
        var room = await uow.Rooms.GetByCodeAsync(roomCode, ct)
            ?? throw new InvalidOperationException($"Room '{roomCode}' không tồn tại");

        if (room.State != RoomState.Lobby)
            throw new InvalidOperationException("Game đã bắt đầu, không thể join");

        var playerCount = await uow.Players.CountByRoomCodeAsync(roomCode, ct);
        if (playerCount >= room.MaxPlayers)
            throw new InvalidOperationException($"Phòng đã đầy ({room.MaxPlayers} người)");

        // Server sinh ClientId nếu client không gửi (lần join đầu)
        var clientId = req.ClientId ?? Guid.NewGuid();

        // Check rejoin: nếu Player.Id đã tồn tại → cập nhật trạng thái online rồi return
        var existing = await uow.Players.GetByIdAsync(clientId, ct);
        if (existing != null)
        {
            if (existing.RoomCode != roomCode)
                throw new InvalidOperationException("Bạn đang ở phòng khác");

            await uow.Players.SetOnlineStatusAsync(clientId, true, ct);
            await notifier.PlayerJoinedAsync(roomCode, mapper.Map<PlayerDto>(existing));
            return new JoinRoomResponse(existing.Id, room.Code, existing.Name, playerCount);
        }

        // Join lần đầu: tạo player mới
        var player = new Player
        {
            Id = clientId,
            RoomCode = roomCode,
            Name = req.PlayerName,
            JoinedAt = DateTime.UtcNow,
            Online = true
        };
        await uow.Players.AddAsync(player, ct);
        await uow.SaveChangesAsync(ct);

        await notifier.PlayerJoinedAsync(roomCode, mapper.Map<PlayerDto>(player));

        logger.LogInformation("Player {PlayerId} ({Name}) joined {Code}",
            player.Id, player.Name, roomCode);

        return new JoinRoomResponse(player.Id, room.Code, player.Name, playerCount + 1);
    }

    public async Task<PickCardResponse> PickCardAsync(
        PickCardRequest req, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var room = await uow.Rooms.GetByCodeAsync(req.RoomCode, ct)
            ?? throw new InvalidOperationException($"Room '{req.RoomCode}' không tồn tại");

        if (room.State != RoomState.Lobby && room.State != RoomState.Picking)
            throw new InvalidOperationException("Không thể pick card lúc này");

        var player = await uow.Players.GetByIdAsync(req.PlayerId, ct)
            ?? throw new InvalidOperationException("Player không tồn tại");

        if (player.RoomCode != req.RoomCode)
            throw new InvalidOperationException("Player không thuộc phòng này");

        if (player.CardId != null)
            throw new InvalidOperationException("Bạn đã pick card rồi");

        var card = await uow.Cards.GetByIdAsync(req.CardId, ct)
            ?? throw new InvalidOperationException("Card không tồn tại");

        if (card.RoomCode != req.RoomCode)
            throw new InvalidOperationException("Card không thuộc phòng này");

        if (card.OwnerId != null)
            return new PickCardResponse(req.CardId, card.Grid, false);   // bị đội khác pick rồi

        // Gán card atomic trong transaction
        card.OwnerId = req.PlayerId;
        player.CardId = req.CardId;

        uow.Cards.Update(card);
        uow.Players.Update(player);

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Player {PlayerId} picked card {CardId} in room {Code}",
            req.PlayerId, req.CardId, req.RoomCode);

        return new PickCardResponse(req.CardId, card.Grid, true);
    }

    public async Task<RoomDto?> GetRoomAsync(string code, CancellationToken ct = default)
    {
        var room = await uow.Rooms.GetWithDetailsAsync(code, ct);
        return room == null ? null : mapper.Map<RoomDto>(room);
    }

    public async Task<List<CardDto>> GetAvailableCardsAsync(
        string roomCode, CancellationToken ct = default)
    {
        var cards = await uow.Cards.GetAvailableAsync(roomCode, ct);
        return mapper.Map<List<CardDto>>(cards);
    }

    public async Task<List<PlayerDto>> GetPlayersAsync(
        string roomCode, CancellationToken ct = default)
    {
        var players = await uow.Players.GetByRoomCodeAsync(roomCode, ct);
        return mapper.Map<List<PlayerDto>>(players);
    }

    public async Task<bool> UnpickCardAsync(
    string roomCode, Guid playerId, CancellationToken ct = default)
    {
        await using var tx = await uow.BeginTransactionAsync(ct);

        var player = await uow.Players.GetByIdAsync(playerId, ct);
        if (player == null || player.RoomCode != roomCode || player.CardId == null)
            return false;

        var card = await uow.Cards.GetByIdAsync(player.CardId.Value, ct);
        if (card != null)
        {
            card.OwnerId = null;
            uow.Cards.Update(card);
        }
        player.CardId = null;
        uow.Players.Update(player);

        await uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Player {Pid} unpicked card in {Code}", playerId, roomCode);
        return true;
    }
}