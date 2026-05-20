using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class CalledNumberRepository(AppDbContext db) : ICalledNumberRepository
{
    public Task<List<CalledNumber>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.CalledNumbers
            .Where(c => c.RoomCode == roomCode)
            .OrderBy(c => c.CalledAt)
            .ToListAsync(ct);

    public Task<List<int>> GetCalledNumbersAsync(string roomCode, CancellationToken ct = default)
        => db.CalledNumbers
            .Where(c => c.RoomCode == roomCode)
            .OrderBy(c => c.CalledAt)
            .Select(c => c.Number)
            .ToListAsync(ct);

    public Task<bool> IsCalledAsync(string roomCode, int number, CancellationToken ct = default)
        => db.CalledNumbers.AnyAsync(c => c.RoomCode == roomCode && c.Number == number, ct);

    public async Task AddAsync(CalledNumber called, CancellationToken ct = default)
        => await db.CalledNumbers.AddAsync(called, ct);
}