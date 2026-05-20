using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class KinhClaimRepository(AppDbContext db) : IKinhClaimRepository
{
    public Task<List<KinhClaim>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.KinhClaims
            .Where(k => k.RoomCode == roomCode)
            .OrderBy(k => k.ClaimedAt)
            .ToListAsync(ct);

    public Task<KinhClaim?> GetLatestByPlayerAsync(
        string roomCode, Guid playerId, CancellationToken ct = default)
        => db.KinhClaims
            .Where(k => k.RoomCode == roomCode && k.PlayerId == playerId)
            .OrderByDescending(k => k.ClaimedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(KinhClaim claim, CancellationToken ct = default)
        => await db.KinhClaims.AddAsync(claim, ct);

    public void Update(KinhClaim claim) => db.KinhClaims.Update(claim);
}