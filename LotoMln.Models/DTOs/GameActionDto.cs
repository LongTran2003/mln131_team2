using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

// Host spin wheel
public record SpinWheelRequest(Guid HostId);
public record SpinWheelResponse(int SpunNumber, QuestionDto Question);

// Host chọn người trả lời
public record SelectAnswererRequest(Guid HostId, Guid PlayerId);

// Host bỏ qua slot (không ai trả lời được)
public record SkipSlotRequest(Guid HostId);

// Host submit câu trả lời thay cho player
public record SubmitAnswerRequest(Guid PlayerId, int AnswerIndex);
public record SubmitAnswerResponse(bool IsCorrect, int CorrectIndex, GamePhase NextPhase, int? CalledNumber);

// Kinh
public record ClaimKinhRequest(Guid PlayerId);
public record KinhVerifyResult(bool IsValid, WinType WinType, int WinIndex, string? Reason);
