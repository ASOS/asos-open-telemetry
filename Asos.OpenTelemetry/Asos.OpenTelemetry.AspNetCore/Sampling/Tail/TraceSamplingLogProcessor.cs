using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// A log processor that filters logs based on the sampling status of the current trace. This allows logs to be recorded only if the current trace is sampled
/// </summary>
public class TraceSamplingLogProcessor : BaseProcessor<LogRecord>
{
    /// <summary>
    /// Custom OnEnd method that checks if the current trace is sampled before allowing the log record to be processed.
    /// Will filter out logs if the current trace is not sampled (i.e., does not have the Recorded flag set).
    /// </summary>
    /// <param name="data">The LogRecord data</param>
    public override void OnEnd(LogRecord data)
    {
        var currentActivity = Activity.Current;

        // If there's no current activity, allow the log (could be application startup, etc.)
        if (currentActivity == null)
        {
            base.OnEnd(data);
            return;
        }

        if (currentActivity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded))
        {
            // Trace is sampled, so allow the log
            base.OnEnd(data);
        }

        // If trace is not sampled (no Recorded flag), we don't call base.OnEnd()
        // which effectively filters out this log record
    }
}