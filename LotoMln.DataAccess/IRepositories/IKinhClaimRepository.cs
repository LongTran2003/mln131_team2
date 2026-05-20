using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface IKinhClaimRepository
{
    Task<List<KinhClaim>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<KinhClaim?> GetLatestByPlayerAsync(string roomCode, Guid playerId, CancellationToken ct = default);
    Task AddAsync(KinhClaim claim, CancellationToken ct = default);
    void Update(KinhClaim claim);
}