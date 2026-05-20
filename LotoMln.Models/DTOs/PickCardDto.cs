namespace LotoMln.Models.DTOs;

public record PickCardRequest(string RoomCode, Guid PlayerId, Guid CardId);
public record PickCardResponse(Guid CardId, int[][] Grid, bool Success);