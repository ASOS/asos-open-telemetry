
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

internal class PendingSpan
{
    public Activity Activity { get; set; } = null!;
    public HttpContext? HttpContext { get; set; }
    public DateTime StartTime { get; set; }
}