using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class GameStateRepository(AppDbContext db) : IGameStateRepository
{
    public Task<GameStateSnapshot?> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.GameStates.FirstOrDefaultAsync(g => g.RoomCode == roomCode, ct);

    public async Task<GameStateSnapshot> GetRequiredAsync(string roomCode, CancellationToken ct = default)
        => await GetByRoomCodeAsync(roomCode, ct)
           ?? throw new InvalidOperationException($"GameState not found for room {roomCode}");

    public async Task AddAsync(GameStateSnapshot state, CancellationToken ct = default)
        => await db.GameStates.AddAsync(state, ct);

    public void Update(GameStateSnapshot state) => db.GameStates.Update(state);
}