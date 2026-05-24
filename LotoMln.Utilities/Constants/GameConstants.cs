namespace LotoMln.Utilities.Constants;

public static class GameConstants
{
    public const int MaxPlayers = 5;
    public const int TotalCards = 10;
    public const int NumberPoolSize = 40;
    public const int CardGridSize = 4;           // 4×4 grid
    public const int NumbersPerColumn = 3;       // 3 số / cột, 1 ô trống / cột
    public const int TurnDurationSec = 15;
    public const int StealTimeoutSec = 10;
    public const int NameMaxLength = 15;
    public const int RoomCodeLength = 6;

    // 4 cột × 10 số = pool 40 số
    public static readonly (int Min, int Max)[] ColumnRanges =
    [
        (1, 10), (11, 20), (21, 30), (31, 40)
    ];
}