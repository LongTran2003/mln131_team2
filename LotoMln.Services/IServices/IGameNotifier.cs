using LotoMln.Models.DTOs;

namespace LotoMln.Services.IServices;

public interface IGameNotifier
{
    Task PlayerJoinedAsync(string roomCode, PlayerDto player);
    Task GameStartedAsync(string roomCode, GameStateDto state);

    // Host quay số → tất cả thấy spun number; question chỉ host nhìn (FE gate)
    Task WheelSpunAsync(string roomCode, int spunNumber, QuestionDto question);

    // Host chọn người trả lời → player đó nhận popup "Đến lượt bạn"
    Task AnswererSelectedAsync(string roomCode, Guid drawerId);

    // Host đánh dấu đáp án → mọi người biết đúng/sai
    Task AnswerSubmittedAsync(string roomCode, Guid playerId, bool isCorrect, int correctIndex);

    // Player thắng câu → mark số tự động
    Task NumberCalledAsync(string roomCode, int number, Guid byPlayer);

    // Host bỏ qua slot (không ai biết)
    Task SlotSkippedAsync(string roomCode, int number);

    Task KinhClaimedAsync(string roomCode, Guid playerId, bool verified, string? reason);
    Task GameEndedAsync(string roomCode, Guid winnerId);
    Task ReadyToSpinAsync(string roomCode);
    Task CardPickedAsync(string roomCode, Guid playerId, Guid cardId, CancellationToken ct = default);
    Task CardUnpickedAsync(string roomCode, Guid playerId, Guid cardId, CancellationToken ct = default);
}
