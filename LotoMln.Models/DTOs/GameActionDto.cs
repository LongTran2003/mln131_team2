using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

// Host spin wheel
public record SpinWheelRequest(Guid HostId);
public record SpinWheelResponse(int SpunNumber, QuestionDto Question, Guid FirstAnswererId, DateTime Deadline);

// Submit answer (P1 trả lời sau khi host spin)
public record SubmitAnswerRequest(Guid PlayerId, int AnswerIndex);
public record SubmitAnswerResponse(bool IsCorrect, int CorrectIndex, GamePhase NextPhase, int? CalledNumber);

// Steal (các đội cướp sau khi P1 sai)
public record StealAttemptRequest(Guid PlayerId, int AnswerIndex);
public record StealAttemptResponse(bool Accepted, DateTime Timestamp);
public record StealResolveResult(Guid? WinnerId, int? CalledNumber, bool SlotLocked);

// Kinh
public record ClaimKinhRequest(Guid PlayerId);
public record KinhVerifyResult(bool IsValid, WinType WinType, int WinIndex, string? Reason);