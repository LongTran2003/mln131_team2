using LotoMln.DataAccess.IRepositories;
using LotoMln.Models.DTOs;
using LotoMln.Models.Enums;
using LotoMln.Services.IServices;

namespace LotoMln.Services.Services;

public class KinhVerifierService(IUnitOfWork uow) : IKinhVerifierService
{
    public async Task<KinhVerifyResult> VerifyAsync(
        string roomCode, Guid playerId, CancellationToken ct = default)
    {
        var player = await uow.Players.GetWithCardAsync(playerId, ct);
        if (player == null)
            return new KinhVerifyResult(false, WinType.Row, -1, "Player không tồn tại");
        if (player.Card == null)
            return new KinhVerifyResult(false, WinType.Row, -1, "Player chưa có card");

        var called = (await uow.CalledNumbers.GetCalledNumbersAsync(roomCode, ct)).ToHashSet();
        var grid = player.Card.Grid;
        const int N = 5;

        // Check rows
        for (int r = 0; r < N; r++)
        {
            if (Enumerable.Range(0, N).All(c => called.Contains(grid[r][c])))
                return new KinhVerifyResult(true, WinType.Row, r, null);
        }

        // Check columns
        for (int c = 0; c < N; c++)
        {
            if (Enumerable.Range(0, N).All(r => called.Contains(grid[r][c])))
                return new KinhVerifyResult(true, WinType.Column, c, null);
        }

        return new KinhVerifyResult(false, WinType.Row, -1, "Chưa đủ 5 ô liên tiếp trong hàng/cột");
    }
}