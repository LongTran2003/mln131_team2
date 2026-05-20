using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Entities;
using LotoMln.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.DataAccess.Repositories;

public class QuestionRepository(AppDbContext db) : IQuestionRepository
{
    public Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Questions.FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<List<Question>> GetRandomByTypeAsync(
        QuestionType type, int count, CancellationToken ct = default)
        => db.Questions
            .Where(q => q.Type == type)
            .OrderBy(q => EF.Functions.Random())   // Npgsql translates to PostgreSQL random()
            .Take(count)
            .ToListAsync(ct);

    public Task<int> CountAsync(QuestionType? type = null, CancellationToken ct = default)
    {
        var query = db.Questions.AsQueryable();
        if (type.HasValue) query = query.Where(q => q.Type == type.Value);
        return query.CountAsync(ct);
    }

    public async Task AddAsync(Question question, CancellationToken ct = default)
        => await db.Questions.AddAsync(question, ct);

    public async Task AddRangeAsync(IEnumerable<Question> questions, CancellationToken ct = default)
        => await db.Questions.AddRangeAsync(questions, ct);
}