namespace LotoMln.Models.Enums;

public enum GamePhase
{
    Idle,             // Host chưa quay — chờ spin
    DrawerAnswering,  // Slot active. CurrentDrawerId=null → host đang chọn người; ≠null → host đang đánh dấu đáp án
    Revealing         // Kết quả 5s trước khi về Idle
}
