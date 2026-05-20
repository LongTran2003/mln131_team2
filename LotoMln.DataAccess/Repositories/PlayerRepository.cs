using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class PlayerRepository(AppDbContext db) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Players.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Player?> GetWithCardAsync(Guid id, CancellationToken ct = default)
        => db.Players.Include(p => p.Card).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Player>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.Players.Where(p => p.RoomCode == roomCode).ToListAsync(ct);

    public Task<int> CountByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.Players.CountAsync(p => p.RoomCode == roomCode, ct);

    public async Task AddAsync(Player player, CancellationToken ct = default)
        => await db.Players.AddAsync(player, ct);

    public void Update(Player player) => db.Players.Update(player);

    public async Task SetOnlineStatusAsync(Guid id, bool online, CancellationToken ct = default)
    {
        // ExecuteUpdate: bypass change tracking, ghi thẳng DB (nhanh hơn cho update đơn lẻ)
        await db.Players
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Online, online), ct);
    }
}