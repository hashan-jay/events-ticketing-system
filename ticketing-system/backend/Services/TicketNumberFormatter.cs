namespace EventsLab.Api.Services;

public static class TicketNumberFormatter
{
    public static string Format(string? prefix, long sequence, int pad = 4)
    {
        var number = sequence.ToString().PadLeft(pad, '0');
        return string.IsNullOrWhiteSpace(prefix) ? number : $"{prefix.Trim()}{number}";
    }

    public static int ResolvePad(long endNumber) => Math.Max(4, endNumber.ToString().Length);
}
