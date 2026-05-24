using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.BackgroundServices;

// Tick every 500ms — chỉ xử lý timeout của phase Revealing.
// DrawerAnswering không có deadline vì host control trực tiếp (không có time pressure).
public class TurnTimeoutHostedService(
    IServiceProvider services,
    ILogger<TurnTimeoutHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "TurnTimeout tick error"); }
            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        await using var scope = services.CreateAsyncScope();
        var engine = scope.ServiceProvider.GetRequiredService<IGameEngineService>();
        var db = scope.ServiceProvider.GetRequiredService<DataAccess.DBContext.AppDbContext>();

        var now = DateTime.UtcNow;
        var expired = await db.GameStates
            .Where(g => g.Phase == GamePhase.Revealing
                        && g.Deadline != null && g.Deadline < now)
            .ToListAsync(ct);

        foreach (var state in expired)
        {
            try
            {
                logger.LogInformation("Room {Code}: revealing ended → Idle", state.RoomCode);
                await engine.AdvanceToIdleAsync(state.RoomCode, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed handling timeout for room {Code}", state.RoomCode);
            }
        }
    }
}