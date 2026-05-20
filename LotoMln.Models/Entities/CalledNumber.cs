namespace LotoMln.Models.Entities;

public class CalledNumber
{
    public string RoomCode { get; set; } = string.Empty;   // composite PK
    public int Number { get; set; }                         // composite PK
    public DateTime CalledAt { get; set; } = DateTime.UtcNow;
    public Guid CalledByPlayerId { get; set; }

    public Room Room { get; set; } = null!;
}