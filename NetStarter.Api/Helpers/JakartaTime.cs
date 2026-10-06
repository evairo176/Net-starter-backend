using System.Runtime.CompilerServices;

namespace NetStarter.Api.Helpers;

/// <summary>
/// Central source of truth for Jakarta (UTC+7) time (copy pola project-management).
/// Tidak ada DST di Asia/Jakarta - aman hardcode UTC+7.
/// </summary>
public static class JakartaTime
{
    private static readonly TimeZoneInfo JakartaZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta") ??
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time") ??
        TimeZoneInfo.CreateCustomTimeZone("Jakarta_Fallback", TimeSpan.FromHours(7), "Jakarta", "Jakarta Standard Time");

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, JakartaZone);
    public static DateTime FromUtc(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, JakartaZone);
    public static DateTime ToUtc(DateTime jakarta) => TimeZoneInfo.ConvertTimeToUtc(jakarta, JakartaZone);
    public static string Format(DateTime dt) => FromUtc(dt).ToString("dd/MM/yyyy HH:mm");
}