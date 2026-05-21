using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.BackgroundServices;

/// <summary>
/// Tick mỗi 500ms, resolve các steal phase đã hết timeout (10s).
/// </summary>
public class StealResolverHostedService(
    IServiceProvider services,
    ILogger<StealResolverHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "StealResolver tick error"); }

            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var engine = scope.ServiceProvider.GetRequiredService<IGameEngineService>();
        var notifier = scope.ServiceProvider.GetRequiredService<IGameNotifier>();
        var db = scope.ServiceProvider.GetRequiredService<DataAccess.DBContext.AppDbContext>();

        var now = DateTime.UtcNow;
        var expired = await db.GameStates
            .Where(g => g.Phase == GamePhase.Stealing && g.Deadline != null && g.Deadline < now)
            .ToListAsync(ct);

        foreach (var state in expired)
        {
            try
            {
                var result = await engine.ResolveStealAsync(state.RoomCode, ct);
                await notifier.StealResolvedAsync(
                    state.RoomCode, result.WinnerId, result.CalledNumber, result.SlotLocked);

                if (result.CalledNumber.HasValue && result.WinnerId.HasValue)
                {
                    await notifier.NumberCalledAsync(
                        state.RoomCode, result.CalledNumber.Value, result.WinnerId.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed resolving steal for room {Code}", state.RoomCode);
            }
        }
    }
}