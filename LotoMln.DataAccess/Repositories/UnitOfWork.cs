using LotoMln.DataAccess.DBContext;
using LotoMln.DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace LotoMln.DataAccess.Repositories;

public class UnitOfWork(
    AppDbContext db,
    IRoomRepository rooms,
    IPlayerRepository players,
    ICardRepository cards,
    IQuestionRepository questions,
    IQuestionSlotRepository questionSlots,
    IGameStateRepository gameStates,
    ICalledNumberRepository calledNumbers,
    IStealAttemptRepository stealAttempts,
    IKinhClaimRepository kinhClaims) : IUnitOfWork
{
    public IRoomRepository Rooms { get; } = rooms;
    public IPlayerRepository Players { get; } = players;
    public ICardRepository Cards { get; } = cards;
    public IQuestionRepository Questions { get; } = questions;
    public IQuestionSlotRepository QuestionSlots { get; } = questionSlots;
    public IGameStateRepository GameStates { get; } = gameStates;
    public ICalledNumberRepository CalledNumbers { get; } = calledNumbers;
    public IStealAttemptRepository StealAttempts { get; } = stealAttempts;
    public IKinhClaimRepository KinhClaims { get; } = kinhClaims;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => db.Database.BeginTransactionAsync(ct);

    public ValueTask DisposeAsync() => db.DisposeAsync();
}