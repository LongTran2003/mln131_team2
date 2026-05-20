namespace LotoMln.Models.Entities;

public class StealAttempt
{
    public Guid Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public Guid SlotId { get; set; }
    public int Answer { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;   // server time (fair race)
    public bool IsCorrect { get; set; }
}