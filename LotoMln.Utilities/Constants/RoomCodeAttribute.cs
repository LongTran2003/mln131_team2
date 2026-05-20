using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LotoMln.Utilities.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class RoomCodeAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is string s && Regex.IsMatch(s, "^[A-Z0-9]{6}$");

    public override string FormatErrorMessage(string name)
        => $"{name} phải là 6 ký tự A-Z hoặc 0-9";
}