using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

public interface IKinhVerifierService
{
    Task<KinhVerifyResult> VerifyAsync(string roomCode, Guid playerId, CancellationToken ct = default);
}