namespace LotoMln.Services.Helpers;

public static class RoomCodeGenerator
{
    // Loại bỏ ký tự dễ nhầm: 0, O, 1, I
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(int length)
    {
        var random = Random.Shared;
        return new string(Enumerable.Range(0, length)
            .Select(_ => Chars[random.Next(Chars.Length)])
            .ToArray());
    }
}