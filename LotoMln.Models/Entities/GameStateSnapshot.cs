using LotoMln.Models.Enums;

namespace LotoMln.Models.Entities;

public class GameStateSnapshot
{
    public string RoomCode { get; set; } = string.Empty;   // PK + FK (1-1 với Room)
    public GamePhase Phase { get; set; } = GamePhase.Idle;
    public Guid? CurrentDrawerId { get; set; }
    public Guid? CurrentSlotId { get; set; }
    public DateTime? PhaseStartedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public List<Guid> PlayerQueue { get; set; } = [];      // jsonb
    public int QueueIndex { get; set; }

    public Room Room { get; set; } = null!;
}