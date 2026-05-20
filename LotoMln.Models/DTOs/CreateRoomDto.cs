using LotoMln.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace LotoMln.Models.DTOs;

public record CreateRoomRequest(
    [Required, ValidPlayerName] string HostName
);

public record CreateRoomResponse(
    string RoomCode,
    Guid HostId,
    string HostName
);