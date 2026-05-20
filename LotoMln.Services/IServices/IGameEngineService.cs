using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

public interface IGameEngineService
{
    Task<GameStateDto> StartGameAsync(string roomCode, Guid initiatorId, CancellationToken ct = default);
    Task<GameStateDto> SelectNextDrawerAsync(string roomCode, CancellationToken ct = default);
    Task<DrawerPickResponse> OnDrawerPicksSlotAsync(string roomCode, DrawerPickRequest req, CancellationToken ct = default);
    Task<SubmitAnswerResponse> OnDrawerAnswersAsync(string roomCode, SubmitAnswerRequest req, CancellationToken ct = default);
    Task<StealAttemptResponse> RecordStealAttemptAsync(string roomCode, StealAttemptRequest req, CancellationToken ct = default);
    Task<StealResolveResult> ResolveStealAsync(string roomCode, CancellationToken ct = default);
    Task<bool> MarkNumberAsync(string roomCode, MarkNumberRequest req, CancellationToken ct = default);
    Task<KinhVerifyResult> ClaimKinhAsync(string roomCode, ClaimKinhRequest req, CancellationToken ct = default);
    Task<GameStateDto?> GetGameStateAsync(string roomCode, CancellationToken ct = default);
}