using LotoMln.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace LotoMln.Models.DTOs;

public record JoinRoomRequest(
    Guid? ClientId,
    [Required, ValidPlayerName] string PlayerName
);

public record JoinRoomResponse(
    Guid PlayerId,
    string RoomCode,
    string Name,
    int PlayerCount
);