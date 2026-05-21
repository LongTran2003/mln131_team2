using LotoMln.Services.IServices;
using LotoMln.Utilities.Constants;
using StackExchange.Redis;

namespace LotoMln.Services.Services;

public class RedisGameCacheService(IConnectionMultiplexer redis) : IGameCacheService
{
    public async Task<bool> TryAcquireKinhLockAsync(
        string roomCode, Guid playerId, TimeSpan ttl)
    {
        var db = redis.GetDatabase();
        var key = RedisKeys.KinhLock(roomCode);

        // SETNX với TTL: chỉ set được nếu key chưa tồn tại
        return await db.StringSetAsync(
            key,
            playerId.ToString(),
            ttl,
            When.NotExists);
    }

    public async Task<Guid?> GetKinhLockOwnerAsync(string roomCode)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync(RedisKeys.KinhLock(roomCode));
        return value.HasValue && Guid.TryParse(value, out var id) ? id : null;
    }
}