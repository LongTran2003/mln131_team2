using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Player?> GetWithCardAsync(Guid id, CancellationToken ct = default);
    Task<List<Player>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<int> CountByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task AddAsync(Player player, CancellationToken ct = default);
    void Update(Player player);
    Task SetOnlineStatusAsync(Guid id, bool online, CancellationToken ct = default);
    Task<Player?> GetByNameInRoomAsync(string roomCode, string name, CancellationToken ct = default);
}