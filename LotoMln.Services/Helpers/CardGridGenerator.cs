using LotoMln.Utilities.Constants;

namespace LotoMln.Services.Helpers;

public static class CardGridGenerator
{
    /// <summary>
    /// Sinh ma trận 4×4 lô tô với "đục lỗ":
    ///   - 4 cột, mỗi cột lấy 3 số từ range (1-10, 11-20, 21-30, 31-40)
    ///   - Mỗi hàng có đúng 1 ô trống (0), mỗi cột có đúng 1 ô trống
    ///   - Số trong mỗi cột sắp xếp tăng dần
    /// </summary>
    public static int[][] Generate()
    {
        var random = Random.Shared;
        int size = GameConstants.CardGridSize;       // 4
        int perCol = GameConstants.NumbersPerColumn; // 3

        // Permutation ngẫu nhiên: blankRows[c] = hàng trống của cột c
        // Đảm bảo mỗi hàng đúng 1 ô trống (Latin square pattern)
        var blankRows = Enumerable.Range(0, size).OrderBy(_ => random.Next()).ToArray();

        var columns = new int[size][];
        for (int c = 0; c < size; c++)
        {
            var (min, max) = GameConstants.ColumnRanges[c];
            var nums = Enumerable.Range(min, max - min + 1)
                .OrderBy(_ => random.Next())
                .Take(perCol)
                .OrderBy(n => n)
                .ToArray();

            columns[c] = new int[size];
            int ni = 0;
            for (int r = 0; r < size; r++)
                columns[c][r] = (r == blankRows[c]) ? 0 : nums[ni++];
        }

        // Transpose columns → rows
        var grid = new int[size][];
        for (int r = 0; r < size; r++)
        {
            grid[r] = new int[size];
            for (int c = 0; c < size; c++)
                grid[r][c] = columns[c][r];
        }
        return grid;
    }
}