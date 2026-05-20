using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

public record RoomDto(
    string Code,
    Guid HostId,
    RoomState State,
    int PlayerCount,
    DateTime CreatedAt
);