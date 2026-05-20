using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LotoMln.Utilities.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class ValidPlayerNameAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return false;
        if (s.Length > 15) return false;
        // Cho phép chữ (kể cả tiếng Việt có dấu), số, space, _, -
        return Regex.IsMatch(s, @"^[\p{L}0-9 _-]+$");
    }

    public override string FormatErrorMessage(string name)
        => $"{name} chỉ chứa chữ, số, khoảng trắng, _ và -, tối đa 15 ký tự";
}