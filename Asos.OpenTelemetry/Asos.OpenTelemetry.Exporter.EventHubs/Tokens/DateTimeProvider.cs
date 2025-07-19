namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal static class SystemTime
{
    internal static Func<DateTime> UtcNow = () => DateTime.UtcNow;

    /// <summary>
    ///     Set time to return when SystemTime.Now() is called.
    /// </summary>
    internal static void SetDateTime(DateTime dateTimeNow)
    {
        UtcNow = () => dateTimeNow;
    }

    /// <summary>
    ///     Resets SystemTime.Now() to return DateTime.Now.
    /// </summary>
    internal static void ResetDateTime()
    {
        UtcNow = () => DateTime.Now;
    }
}