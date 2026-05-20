namespace LotoMln.Models.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int[][] Grid { get; set; } = [];   // 5x5, lưu jsonb
    public Guid? OwnerId { get; set; }

    public Room Room { get; set; } = null!;
}