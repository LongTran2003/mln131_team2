using LotoMln.Models.Entities;
using LotoMln.Models.Enums;

namespace LotoMln.DataAccess.IRepositories;

public interface IQuestionSlotRepository
{
    Task<QuestionSlot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<QuestionSlot?> GetByIdWithQuestionAsync(Guid id, CancellationToken ct = default);
    Task<QuestionSlot?> GetByPositionAsync(string roomCode, int position, CancellationToken ct = default);
    Task<List<QuestionSlot>> GetByRoomCodeAsync(string roomCode, CancellationToken ct = default);
    Task<int> CountByStatusAsync(string roomCode, SlotStatus status, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<QuestionSlot> slots, CancellationToken ct = default);
    void Update(QuestionSlot slot);
}