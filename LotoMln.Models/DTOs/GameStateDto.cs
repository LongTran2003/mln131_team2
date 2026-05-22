using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

public record GameStateDto(
    string RoomCode,
    GamePhase Phase,
    Guid? CurrentDrawerId,
    Guid? CurrentSlotId,
    DateTime? Deadline,
    List<int> CalledNumbers,
    int RemainingSlots,
    int LockedSlots,
    List<int> AnsweredPositions,     // ← thêm
    List<int> LockedPositions         // ← thêm
);