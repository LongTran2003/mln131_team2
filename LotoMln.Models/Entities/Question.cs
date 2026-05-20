using LotoMln.Models.Enums;

namespace LotoMln.Models.Entities;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string[] Options { get; set; } = [];   // 4 phần tử, jsonb
    public int CorrectIndex { get; set; }
    public QuestionType Type { get; set; }
    public string Source { get; set; } = string.Empty;   // "VK XIV T1 tr.47"
}