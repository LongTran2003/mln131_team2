using Microsoft.EntityFrameworkCore.Storage;

namespace LotoMln.DataAccess.IRepositories;

public interface IUnitOfWork : IAsyncDisposable
{
    IRoomRepository Rooms { get; }
    IPlayerRepository Players { get; }
    ICardRepository Cards { get; }
    IQuestionRepository Questions { get; }
    IQuestionSlotRepository QuestionSlots { get; }
    IGameStateRepository GameStates { get; }
    ICalledNumberRepository CalledNumbers { get; }
    IStealAttemptRepository StealAttempts { get; }
    IKinhClaimRepository KinhClaims { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}