using LotoMln.Models.Enums;

namespace LotoMln.Models.Entities;

public class QuestionSlot
{
    public Guid Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int Position { get; set; }                    // 1-40
    public Guid QuestionId { get; set; }
    public int AssignedNumber { get; set; }              // 1-40
    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public Guid? AnsweredByPlayerId { get; set; }

    public Room Room { get; set; } = null!;
    public Question Question { get; set; } = null!;
}