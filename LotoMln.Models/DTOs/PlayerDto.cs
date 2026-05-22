namespace LotoMln.Models.DTOs;

public record PlayerDto(
    Guid Id,
    string Name,
    bool IsHost,
    Guid? CardId,
    bool Online,
    List<int> MarkedNumbers
);