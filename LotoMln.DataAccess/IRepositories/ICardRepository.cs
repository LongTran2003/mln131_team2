using LotoMln.Models.Entities;

namespace LotoMln.DataAccess.IRepositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Card>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<List<Card>> GetAvailableAsync(string roomCode, CancellationToken ct = default);
    Task AddAsync(Card card, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Card> cards, CancellationToken ct = default);
    void Update(Card card);
}