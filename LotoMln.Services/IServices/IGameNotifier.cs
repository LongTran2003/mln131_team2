using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

/// <summary>
/// Abstraction để Services emit events ra real-time clients mà không cần biết SignalR.
/// Impl thực tế ở API project (SignalRGameNotifier).
/// </summary>
public interface IGameNotifier
{
    Task PlayerJoinedAsync(string roomCode, PlayerDto player);
    Task GameStartedAsync(string roomCode, GameStateDto state);
    Task TurnStartedAsync(string roomCode, Guid drawerId, DateTime deadline);
    Task QuestionShownAsync(string roomCode, Guid drawerId, QuestionDto question, int assignedNumber, DateTime deadline);
    Task AnswerSubmittedAsync(string roomCode, Guid playerId, bool isCorrect, int correctIndex);
    Task NumberCalledAsync(string roomCode, int number, Guid byPlayer);
    Task StealModeStartedAsync(string roomCode, DateTime deadline);
    Task StealResolvedAsync(string roomCode, Guid? winnerId, int? calledNumber, bool slotLocked);
    Task KinhClaimedAsync(string roomCode, Guid playerId, bool verified, string? reason);
    Task GameEndedAsync(string roomCode, Guid winnerId);
}