namespace LotoMln.Models.DTOs;

public record PlayerDto(
    Guid Id,
    string Name,
    Guid? CardId,
    bool Online,
    List<int> MarkedNumbers
);