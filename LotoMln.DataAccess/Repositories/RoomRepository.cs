using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class RoomRepository(AppDbContext db) : IRoomRepository
{
    public Task<Room?> GetByCodeAsync(string code, CancellationToken ct = default)
        => db.Rooms.FirstOrDefaultAsync(r => r.Code == code, ct);

    public Task<Room?> GetWithDetailsAsync(string code, CancellationToken ct = default)
        => db.Rooms
            .Include(r => r.Players)
            .Include(r => r.GameState)
            .FirstOrDefaultAsync(r => r.Code == code, ct);

    public Task<bool> ExistsAsync(string code, CancellationToken ct = default)
        => db.Rooms.AnyAsync(r => r.Code == code, ct);

    public async Task AddAsync(Room room, CancellationToken ct = default)
        => await db.Rooms.AddAsync(room, ct);

    public void Update(Room room) => db.Rooms.Update(room);

    public void Remove(Room room) => db.Rooms.Remove(room);
}