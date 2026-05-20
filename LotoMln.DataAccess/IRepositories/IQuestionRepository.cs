using LotoMln.Models.Entities;
using LotoMln.Models.Enums;

namespace LotoMln.DataAccess.IRepositories;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Question>> GetRandomByTypeAsync(QuestionType type, int count, CancellationToken ct = default);
    Task<int> CountAsync(QuestionType? type = null, CancellationToken ct = default);
    Task AddAsync(Question question, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Question> questions, CancellationToken ct = default);
}