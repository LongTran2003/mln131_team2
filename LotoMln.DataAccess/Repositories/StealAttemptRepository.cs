using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class StealAttemptRepository(AppDbContext db) : IStealAttemptRepository
{
    public Task<List<StealAttempt>> GetBySlotAsync(
        string roomCode, Guid slotId, CancellationToken ct = default)
        => db.StealAttempts
            .Where(s => s.RoomCode == roomCode && s.SlotId == slotId)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);

    public Task<StealAttempt?> GetFirstCorrectAsync(
        string roomCode, Guid slotId, CancellationToken ct = default)
        => db.StealAttempts
            .Where(s => s.RoomCode == roomCode && s.SlotId == slotId && s.IsCorrect)
            .OrderBy(s => s.Timestamp)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(StealAttempt attempt, CancellationToken ct = default)
        => await db.StealAttempts.AddAsync(attempt, ct);
}