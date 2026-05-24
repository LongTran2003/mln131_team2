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

        // KINH dựa vào markedNumbers của player (chỉ số player đã thắng câu hỏi)
        var marked = new HashSet<int>(player.MarkedNumbers);
        var grid = player.Card.Grid;
        int size = LotoMln.Utilities.Constants.GameConstants.CardGridSize; // 4

        // Check rows: tất cả ô != 0 trong hàng phải được mark
        for (int r = 0; r < size; r++)
        {
            var rowNums = Enumerable.Range(0, size).Select(c => grid[r][c]).Where(n => n != 0).ToList();
            if (rowNums.Count > 0 && rowNums.All(n => marked.Contains(n)))
                return new KinhVerifyResult(true, WinType.Row, r, null);
        }

        // Check columns: tất cả ô != 0 trong cột phải được mark
        for (int c = 0; c < size; c++)
        {
            var colNums = Enumerable.Range(0, size).Select(r => grid[r][c]).Where(n => n != 0).ToList();
            if (colNums.Count > 0 && colNums.All(n => marked.Contains(n)))
                return new KinhVerifyResult(true, WinType.Column, c, null);
        }

        return new KinhVerifyResult(false, WinType.Row, -1, "Chưa đủ số hoàn thành hàng hoặc cột");
    }
}