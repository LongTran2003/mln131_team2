namespace LotoMln.Models.Enums;

public enum GamePhase
{
    Idle,             // Host chưa quay — chờ spin
    DrawerAnswering,  // P1 đang trả lời sau khi host spin
    Stealing,         // P1 sai → các đội còn lại cướp
    Revealing         // Kết quả 5s trước khi về Idle
}