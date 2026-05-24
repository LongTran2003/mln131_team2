using LotoMln.API.Hubs;
using LotoMln.Models.DTOs;
using LotoMln.Services.IServices;
using Microsoft.AspNetCore.SignalR;

namespace LotoMln.API.Notifiers;

public class SignalRGameNotifier(IHubContext<GameHub> hub) : IGameNotifier
{
    private static string RoomGroup(string code) => $"room:{code}";

    public Task PlayerJoinedAsync(string roomCode, PlayerDto player)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("PlayerJoined", player);

    public Task GameStartedAsync(string roomCode, GameStateDto state)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("GameStarted", state);

    public Task WheelSpunAsync(string roomCode, int spunNumber, QuestionDto question, Guid firstAnswererId, DateTime deadline)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("WheelSpun",
            new { spunNumber, question, firstAnswererId, deadline });

    public Task ReadyToSpinAsync(string roomCode)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("ReadyToSpin");

    public Task AnswerSubmittedAsync(string roomCode, Guid playerId, bool isCorrect, int correctIndex)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("AnswerSubmitted",
            new { playerId, isCorrect, correctIndex });

    public Task NumberCalledAsync(string roomCode, int number, Guid byPlayer)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("NumberCalled", new { number, byPlayer });

    public Task StealModeStartedAsync(string roomCode, DateTime deadline)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("StealModeStarted", new { deadline });

    public Task StealResolvedAsync(string roomCode, Guid? winnerId, int? calledNumber, bool slotLocked)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("StealResolved",
            new { winnerId, calledNumber, slotLocked });

    public Task KinhClaimedAsync(string roomCode, Guid playerId, bool verified, string? reason)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("KinhClaimed",
            new { playerId, verified, reason });

    public Task GameEndedAsync(string roomCode, Guid winnerId)
        => hub.Clients.Group(RoomGroup(roomCode)).SendAsync("GameEnded", new { winnerId });

    public Task CardPickedAsync(string code, Guid playerId, Guid cardId, CancellationToken ct = default)
        => hub.Clients.Group(RoomGroup(code)).SendAsync("CardPicked", new { playerId, cardId }, ct);

    public Task CardUnpickedAsync(string code, Guid playerId, Guid cardId, CancellationToken ct = default)
        => hub.Clients.Group(RoomGroup(code)).SendAsync("CardUnpicked", new { playerId, cardId }, ct);
}