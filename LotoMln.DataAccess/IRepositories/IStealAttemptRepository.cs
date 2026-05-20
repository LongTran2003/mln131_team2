using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface IStealAttemptRepository
{
    Task<List<StealAttempt>> GetBySlotAsync(string roomCode, Guid slotId, CancellationToken ct = default);
    Task<StealAttempt?> GetFirstCorrectAsync(string roomCode, Guid slotId, CancellationToken ct = default);
    Task AddAsync(StealAttempt attempt, CancellationToken ct = default);
}