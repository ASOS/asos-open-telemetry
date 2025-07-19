using System.Collections.Concurrent;
using System.Diagnostics;
using OpenTelemetry;

namespace Asos.OpenTelemetry.AspNetCore.Tests;

/// <summary>
/// Test exporter that captures exported activities for verification
/// </summary>
public class TestExporter(ConcurrentBag<Activity> exportedActivities) : BaseExporter<Activity>
{
    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (var activity in batch)
        {
            // Only export activities that are marked as recorded (sampled)
            if ((activity.ActivityTraceFlags & ActivityTraceFlags.Recorded) != 0)
            {
                exportedActivities.Add(activity);
            }
        }
        return ExportResult.Success;
    }
}
