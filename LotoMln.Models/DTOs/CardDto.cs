namespace LotoMln.Models.DTOs;

public record CardDto(
    Guid Id,
    int[][] Grid,
    Guid? OwnerId,
    bool IsAvailable
);