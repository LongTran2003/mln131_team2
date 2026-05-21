using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.DTOs;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.BackgroundServices;

/// <summary>
/// Tick mỗi 500ms, tìm GameState đã quá deadline ở phase DrawerSelecting/DrawerAnswering
/// và auto-transition (skip drawer hoặc treat as wrong answer).
/// </summary>
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
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var engine = scope.ServiceProvider.GetRequiredService<IGameEngineService>();
        var notifier = scope.ServiceProvider.GetRequiredService<IGameNotifier>();

        // Query expired states (phase DrawerSelecting hoặc DrawerAnswering, deadline < now)
        // Vì không có repo cho query này, dùng AppDbContext trực tiếp qua scope
        var db = scope.ServiceProvider.GetRequiredService<DataAccess.DBContext.AppDbContext>();
        var now = DateTime.UtcNow;
        var expired = await db.GameStates
            .Where(g => (g.Phase == GamePhase.DrawerSelecting || g.Phase == GamePhase.DrawerAnswering)
                        && g.Deadline != null && g.Deadline < now)
            .ToListAsync(ct);

        foreach (var state in expired)
        {
            try
            {
                if (state.Phase == GamePhase.DrawerSelecting)
                {
                    // Drawer không pick slot trong 15s → skip drawer
                    logger.LogInformation("Room {Code}: drawer {Did} timeout selecting, skip",
                        state.RoomCode, state.CurrentDrawerId);
                    var newState = await engine.SelectNextDrawerAsync(state.RoomCode, ct);
                    await notifier.TurnStartedAsync(
                        state.RoomCode, newState.CurrentDrawerId!.Value, newState.Deadline!.Value);
                }
                else if (state.Phase == GamePhase.DrawerAnswering)
                {
                    // Drawer không trả lời → treat as wrong, vào steal mode
                    logger.LogInformation("Room {Code}: drawer {Did} timeout answering → steal mode",
                        state.RoomCode, state.CurrentDrawerId);
                    var dummy = new SubmitAnswerRequest(state.CurrentDrawerId!.Value, -1);
                    await engine.OnDrawerAnswersAsync(state.RoomCode, dummy, ct);
                    await notifier.AnswerSubmittedAsync(
                        state.RoomCode, state.CurrentDrawerId.Value, false, -1);
                    await notifier.StealModeStartedAsync(state.RoomCode,
                        now.AddSeconds(Utilities.Constants.GameConstants.StealTimeoutSec));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed handling timeout for room {Code}", state.RoomCode);
            }
        }
    }
}