using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

public interface IGameEngineService
{
    Task<GameStateDto> StartGameAsync(string roomCode, Guid initiatorId, CancellationToken ct = default);
    Task<SpinWheelResponse> SpinWheelAsync(string roomCode, Guid hostId, CancellationToken ct = default);
    Task SelectAnswererAsync(string roomCode, Guid hostId, Guid playerId, CancellationToken ct = default);
    Task<SubmitAnswerResponse> OnDrawerAnswersAsync(string roomCode, SubmitAnswerRequest req, CancellationToken ct = default);
    Task SkipSlotAsync(string roomCode, Guid hostId, CancellationToken ct = default);
    Task<KinhVerifyResult> ClaimKinhAsync(string roomCode, ClaimKinhRequest req, CancellationToken ct = default);
    Task<GameStateDto?> GetGameStateAsync(string roomCode, CancellationToken ct = default);
    Task AdvanceToIdleAsync(string roomCode, CancellationToken ct = default);
}
