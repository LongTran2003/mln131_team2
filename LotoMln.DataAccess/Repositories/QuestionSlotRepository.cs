using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using LotoMln.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class QuestionSlotRepository(AppDbContext db) : IQuestionSlotRepository
{
    public Task<QuestionSlot?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.QuestionSlots.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<QuestionSlot?> GetByIdWithQuestionAsync(Guid id, CancellationToken ct = default)
        => db.QuestionSlots.Include(s => s.Question).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<QuestionSlot?> GetByPositionAsync(
        string roomCode, int position, CancellationToken ct = default)
        => db.QuestionSlots.FirstOrDefaultAsync(
            s => s.RoomCode == roomCode && s.Position == position, ct);

    public Task<List<QuestionSlot>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default)
        => db.QuestionSlots.Where(s => s.RoomCode == roomCode).ToListAsync(ct);

    public Task<int> CountByStatusAsync(
        string roomCode, SlotStatus status, CancellationToken ct = default)
        => db.QuestionSlots.CountAsync(s => s.RoomCode == roomCode && s.Status == status, ct);

    public async Task AddRangeAsync(IEnumerable<QuestionSlot> slots, CancellationToken ct = default)
        => await db.QuestionSlots.AddRangeAsync(slots, ct);

    public void Update(QuestionSlot slot) => db.QuestionSlots.Update(slot);
}