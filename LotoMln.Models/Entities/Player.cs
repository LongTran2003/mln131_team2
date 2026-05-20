namespace LotoMln.Models.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CardId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool Online { get; set; } = true;
    public bool UsedRedemption { get; set; }
    public List<int> MarkedNumbers { get; set; } = [];

    public Room Room { get; set; } = null!;
    public Card? Card { get; set; }
}