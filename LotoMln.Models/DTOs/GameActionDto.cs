using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

// Drawer pick slot (chọn ô ?)
public record DrawerPickRequest(Guid PlayerId, int Position);
public record DrawerPickResponse(QuestionDto Question, int AssignedNumber, DateTime Deadline);

// Submit answer
public record SubmitAnswerRequest(Guid PlayerId, int AnswerIndex);
public record SubmitAnswerResponse(bool IsCorrect, int CorrectIndex, GamePhase NextPhase, int? CalledNumber);

// Steal (cướp)
public record StealAttemptRequest(Guid PlayerId, int AnswerIndex);
public record StealAttemptResponse(bool Accepted, DateTime Timestamp);
public record StealResolveResult(Guid? WinnerId, int? CalledNumber, bool SlotLocked);

// Mark + Kinh
public record MarkNumberRequest(Guid PlayerId, int Number);
public record ClaimKinhRequest(Guid PlayerId);
public record KinhVerifyResult(bool IsValid, WinType WinType, int WinIndex, string? Reason);