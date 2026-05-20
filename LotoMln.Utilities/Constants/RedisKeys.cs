namespace LotoMln.Utilities.Constants;

public static class RedisKeys
{
    public static string KinhLock(string roomCode) => $"lock:kinh:{roomCode}";
    public static string GameState(string roomCode) => $"state:{roomCode}";
    public static string StealAttempts(string roomCode) => $"steal:{roomCode}";
}