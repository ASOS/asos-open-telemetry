namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal static class SystemTime
{
    internal static Func<DateTime> UtcNow = () => DateTime.UtcNow;

    /// <summary>
    ///     Set time to return when SystemTime.UtcNow() is called.
    /// </summary>
    internal static void SetDateTime(DateTime dateTimeNow)
    {
        UtcNow = () => dateTimeNow;
    }

    /// <summary>
    ///     Resets SystemTime.UtcNow() to return DateTime.UtcNow.
    /// </summary>
    internal static void ResetDateTime()
    {
        UtcNow = () => DateTime.UtcNow;
    }
}