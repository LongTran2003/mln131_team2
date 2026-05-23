using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.DTOs;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace LotoMln.API.BackgroundServices;

/// <summary>
/// Tick mỗi 500ms, tìm GameState đã quá deadline ở 3 phase:
/// - DrawerSelecting: drawer không pick → skip sang drawer kế
/// - DrawerAnswering: drawer không trả lời → treat as wrong → steal mode
/// - Revealing: hết 5s "đang kiểm tra" → bắt đầu lượt mới
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
        var engine = scope.ServiceProvider.GetRequiredService<IGameEngineService>();
        var db = scope.ServiceProvider.GetRequiredService<DataAccess.DBContext.AppDbContext>();

        var now = DateTime.UtcNow;
        var expired = await db.GameStates
            .Where(g => (g.Phase == GamePhase.DrawerSelecting
                      || g.Phase == GamePhase.DrawerAnswering
                      || g.Phase == GamePhase.Revealing)        // ← thêm
                        && g.Deadline != null && g.Deadline < now)
            .ToListAsync(ct);

        foreach (var state in expired)
        {
            try
            {
                if (state.Phase == GamePhase.DrawerSelecting)
                {
                    // Drawer không pick slot trong 15s → skip
                    logger.LogInformation("Room {Code}: drawer {Did} timeout selecting → skip",
                        state.RoomCode, state.CurrentDrawerId);
                    await engine.SelectNextDrawerAsync(state.RoomCode, ct);
                    // SelectNextDrawerAsync đã notify TurnStarted bên trong → KHÔNG notify lại
                }
                else if (state.Phase == GamePhase.DrawerAnswering)
                {
                    // Drawer không trả lời → treat as wrong (AnswerIndex = -1)
                    logger.LogInformation("Room {Code}: drawer {Did} timeout answering → steal mode",
                        state.RoomCode, state.CurrentDrawerId);
                    var dummy = new SubmitAnswerRequest(state.CurrentDrawerId!.Value, -1);
                    await engine.OnDrawerAnswersAsync(state.RoomCode, dummy, ct);
                    // OnDrawerAnswersAsync đã notify AnswerSubmitted + StealModeStarted bên trong
                }
                else if (state.Phase == GamePhase.Revealing)
                {
                    // ← NHÁNH MỚI: Revealing 5s xong → next turn
                    logger.LogInformation("Room {Code}: revealing ended → advance to next turn",
                        state.RoomCode);
                    await engine.SelectNextDrawerAsync(state.RoomCode, ct);
                    // SelectNextDrawerAsync đã notify TurnStarted bên trong
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed handling timeout for room {Code}", state.RoomCode);
            }
        }
    }
}