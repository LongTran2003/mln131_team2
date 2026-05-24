using LotoMln.Models.DTOs;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.BackgroundServices;

// Tick every 500ms, handle expired deadlines:
// - DrawerAnswering: no answer in time → treat as wrong → steal mode
// - Stealing: steal window closed → resolve steal
// - Revealing: 5s display over → back to Idle
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
            .Where(g => (g.Phase == GamePhase.DrawerAnswering
                      || g.Phase == GamePhase.Stealing
                      || g.Phase == GamePhase.Revealing)
                        && g.Deadline != null && g.Deadline < now)
            .ToListAsync(ct);

        foreach (var state in expired)
        {
            try
            {
                if (state.Phase == GamePhase.DrawerAnswering)
                {
                    logger.LogInformation("Room {Code}: drawer {Did} timeout → steal mode",
                        state.RoomCode, state.CurrentDrawerId);
                    var dummy = new SubmitAnswerRequest(state.CurrentDrawerId!.Value, -1);
                    await engine.OnDrawerAnswersAsync(state.RoomCode, dummy, ct);
                }
                else if (state.Phase == GamePhase.Stealing)
                {
                    logger.LogInformation("Room {Code}: steal window closed → resolving",
                        state.RoomCode);
                    await engine.ResolveStealAsync(state.RoomCode, ct);
                }
                else if (state.Phase == GamePhase.Revealing)
                {
                    logger.LogInformation("Room {Code}: revealing ended → Idle",
                        state.RoomCode);
                    await engine.AdvanceToIdleAsync(state.RoomCode, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed handling timeout for room {Code}", state.RoomCode);
            }
        }
    }
}