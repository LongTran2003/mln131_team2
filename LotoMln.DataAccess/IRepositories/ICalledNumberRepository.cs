using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface ICalledNumberRepository
{
    Task<List<CalledNumber>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<List<int>> GetCalledNumbersAsync(string roomCode, CancellationToken ct = default);
    Task<bool> IsCalledAsync(string roomCode, int number, CancellationToken ct = default);
    Task AddAsync(CalledNumber called, CancellationToken ct = default);
}