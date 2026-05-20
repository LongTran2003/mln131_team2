using LotoMln.Models.Enums;

namespace LotoMln.Models.DTOs;

public record QuestionDto(
    Guid Id,
    string Text,
    string[] Options,
    QuestionType Type
);