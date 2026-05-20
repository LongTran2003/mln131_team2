using LotoMln.Models.Enums;

namespace LotoMln.Models.Entities;

public class KinhClaim
{
    public Guid Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    public bool Verified { get; set; }
    public WinType WinType { get; set; }
    public int WinIndex { get; set; }   // 0-4
}