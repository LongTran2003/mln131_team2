namespace LotoMln.Services.IServices;

public interface IGameCacheService
{
    /// <summary>
    /// Atomic SETNX: thử lấy lock cho player đầu tiên claim Kinh.
    /// Return true = player này thắng race, được verify; false = người khác đã claim trước.
    /// </summary>
    Task<bool> TryAcquireKinhLockAsync(string roomCode, Guid playerId, TimeSpan ttl);

    Task<Guid?> GetKinhLockOwnerAsync(string roomCode);
}