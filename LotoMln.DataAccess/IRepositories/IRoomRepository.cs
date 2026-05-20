using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface IRoomRepository
{
    Task<Room?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Room?> GetWithDetailsAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    void Update(Room room);
    void Remove(Room room);
}