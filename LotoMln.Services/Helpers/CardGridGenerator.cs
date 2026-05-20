using LotoMln.Utilities.Constants;

namespace LotoMln.Services.Helpers;

public static class CardGridGenerator
{
    /// <summary>
    /// Sinh ma trận 5x5 với column ranges 1-8/9-16/.../33-40.
    /// Mỗi cột có 5 số tăng dần từ trên xuống.
    /// </summary>
    public static int[][] Generate()
    {
        var random = Random.Shared;

        // Sinh từng cột trước
        var columns = new int[GameConstants.CardSize][];
        for (int c = 0; c < GameConstants.CardSize; c++)
        {
            var (min, max) = GameConstants.ColumnRanges[c];
            columns[c] = Enumerable.Range(min, max - min + 1)
                .OrderBy(_ => random.Next())          // shuffle 8 số
                .Take(GameConstants.CardSize)         // lấy 5
                .OrderBy(n => n)                       // sort ascending
                .ToArray();
        }

        // Transpose columns → rows (vì entity lưu row-major)
        var grid = new int[GameConstants.CardSize][];
        for (int r = 0; r < GameConstants.CardSize; r++)
        {
            grid[r] = new int[GameConstants.CardSize];
            for (int c = 0; c < GameConstants.CardSize; c++)
                grid[r][c] = columns[c][r];
        }

        return grid;
    }
}