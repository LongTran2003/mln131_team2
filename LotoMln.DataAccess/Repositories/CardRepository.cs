using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class CardRepository(AppDbContext db) : ICardRepository
{
    public Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Cards.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Card>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.Cards.Where(c => c.RoomCode == roomCode).ToListAsync(ct);

    public Task<List<Card>> GetAvailableAsync(string roomCode, CancellationToken ct = default)
        => db.Cards.Where(c => c.RoomCode == roomCode && c.OwnerId == null).ToListAsync(ct);

    public async Task AddAsync(Card card, CancellationToken ct = default)
        => await db.Cards.AddAsync(card, ct);

    public async Task AddRangeAsync(IEnumerable<Card> cards, CancellationToken ct = default)
        => await db.Cards.AddRangeAsync(cards, ct);

    public void Update(Card card) => db.Cards.Update(card);
}