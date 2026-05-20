using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface IGameStateRepository
{
    Task<GameStateSnapshot?> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<GameStateSnapshot> GetRequiredAsync(string roomCode, CancellationToken ct = default);
    Task AddAsync(GameStateSnapshot state, CancellationToken ct = default);
    void Update(GameStateSnapshot state);
}