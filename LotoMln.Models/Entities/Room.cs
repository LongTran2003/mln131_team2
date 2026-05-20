using LotoMln.Models.Enums;

namespace LotoMln.Models.Entities;

public class Room
{
    public string Code { get; set; } = string.Empty;
    public Guid HostId { get; set; }
    public RoomState State { get; set; } = RoomState.Lobby;
    public int TurnNumber { get; set; }
    public Guid? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Settings (denormalized vào Room cho đơn giản)
    public int MaxPlayers { get; set; } = 40;
    public int TurnDurationSec { get; set; } = 25;
    public int StealTimeoutSec { get; set; } = 10;
    public int NameMaxLength { get; set; } = 15;

    // Navigation
    public List<Player> Players { get; set; } = [];
    public List<Card> Cards { get; set; } = [];
    public List<QuestionSlot> QuestionPool { get; set; } = [];
    public GameStateSnapshot? GameState { get; set; }
    public List<CalledNumber> CalledNumbers { get; set; } = [];
    public List<KinhClaim> KinhClaims { get; set; } = [];
}