namespace LotoMln.Utilities.Constants;

public static class GameConstants
{
    public const int MaxPlayers = 5;
    public const int TotalCards = 10;
    public const int NumberPoolSize = 40;
    public const int CardSize = 5;                          // 5x5 grid
    public const int TurnDurationSec = 15;
    public const int StealTimeoutSec = 10;
    public const int RedemptionTriggerLockedCount = 5;
    public const int NameMaxLength = 15;
    public const int RoomCodeLength = 6;

    // 5 cột × dải 8 số = pool 40 số
    public static readonly (int Min, int Max)[] ColumnRanges =
    [
        (1, 8), (9, 16), (17, 24), (25, 32), (33, 40)
    ];
}