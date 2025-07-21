using OpenTelemetry;

namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

/// <summary>
/// A tail-based sampling processor that makes sampling decisions based on span outcomes
/// such as HTTP status codes, exceptions, and dependency failures.
/// 
/// Note: This processor modifies the Activity's ActivityTraceFlags to control sampling
/// rather than dropping spans from the pipeline, as processors cannot drop spans.
/// </summary>
public class TailBasedSamplingProcessor : BaseProcessor<Activity>
{
    // Check for any error-related tags that indicate failure
    private static readonly List<string> ErrorTags =
    [
        "error.type",
        "error.message",
        "exception.type",
        "exception.message",
        "db.error",
        "messaging.error",
        "rpc.error"
    ];

    private readonly ConcurrentDictionary<string, PendingSpan> _pendingSpans = new();
    private readonly TailSamplingOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the TailBasedSamplingProcessor with the specified options and HTTP context accessor.
    /// This processor will use the provided configuration to make sampling decisions based on span outcomes
    /// such as HTTP status codes, exceptions, dependency failures, and request duration.
    /// </summary>
    /// <param name="options">The tail sampling configuration options that define sampling rates and rules for different scenarios.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor used to retrieve request information for route-based sampling decisions.</param>
    public TailBasedSamplingProcessor(TailSamplingOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Called when an activity (span) starts. This method captures the activity and associated HTTP context
    /// for later evaluation when the activity ends. The span is stored in a pending state until its outcome
    /// can be determined, allowing for tail-based sampling decisions.
    /// </summary>
    /// <param name="activity">The activity that is starting, which will be stored for later sampling decision.</param>
    public override void OnStart(Activity activity)
    {
        // Store the span for later decision making
        var pendingSpan = new PendingSpan
        {
            Activity = activity,
            HttpContext = _httpContextAccessor.HttpContext,
            StartTime = DateTime.UtcNow
        };

        _pendingSpans.TryAdd(activity.Id!, pendingSpan);
    }

    /// <summary>
    /// Called when an activity (span) ends. This method evaluates the completed span's outcome
    /// (status codes, exceptions, dependencies, duration) to make a tail-based sampling decision.
    /// If the span should not be sampled, it modifies the Activity's trace flags to mark it as not sampled.
    /// </summary>
    /// <param name="activity">The completed activity to evaluate for sampling based on its final state and outcome.</param>
    public override void OnEnd(Activity activity)
    {
        if (!_pendingSpans.TryRemove(activity.Id!, out var pendingSpan))
        {
            // If we don't have the pending span, forward as-is
            base.OnEnd(activity);
            return;
        }

        var decision = ShouldSampleBasedOnOutcome(activity);
        if (!decision.ShouldSample)
        {
            // Mark the activity as not sampled by clearing the Sampled flag
            // This prevents exporters from exporting it
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
        else
        {
            // While this processor isn't specifically coupled to the application insights
            // exporter, we set these tags to align with the expected format and set the 
            // sampling rate that is captured if that the Azure Monitor exporter is utilised.
            activity.SetTag("_MS.sampleRate", decision.SampleRate);
            activity.SetTag("_MS.itemCount", decision.SampleRate == 0 ? 0 : 100.0 / decision.SampleRate);
        }

        // Always forward to the next processor
        base.OnEnd(activity);
    }

    private SamplingDecisionResult ShouldSampleBasedOnOutcome(Activity activity)
    {
        // Check for exceptions first (highest priority)
        if (HasException(activity))
        {
            return ShouldSample(_options.DefaultExceptionSamplingRate);
        }

        // Check for slow requests (high priority for performance monitoring)
        var duration = activity.Duration;
        if (duration > _options.SlowRequestThreshold)
        {
            return ShouldSampleForSlowRequest();
        }

        // Check for dependency failures
        if (HasDependencyFailure(activity))
        {
            return ShouldSampleForDependencyFailure();
        }

        // Check HTTP status codes (after route rules)
        if (TryGetHttpStatusCode(activity, out var statusCode))
        {
            return ShouldSampleForHttpStatus(statusCode);
        }

        // Fall back to default sampling rate
        return ShouldSample(_options.DefaultSamplingRate);
    }

    private static bool HasException(Activity activity)
    {
        return activity.GetTagItem("exception.type") != null ||
               activity.GetTagItem("exception.message") != null ||
               activity.Status == ActivityStatusCode.Error;
    }

    private bool TryGetHttpStatusCode(Activity activity, out int statusCode)
    {
        statusCode = 0;
        var statusCodeTag = activity.GetTagItem("http.status_code")?.ToString() ??
                            activity.GetTagItem("http.response.status_code")?.ToString();

        return int.TryParse(statusCodeTag, out statusCode);
    }

    private SamplingDecisionResult ShouldSampleForHttpStatus(int statusCode)
    {
        var rule = _options.StatusCodeRules
            .FirstOrDefault(r => r.StatusCode == statusCode || IsInRange(statusCode, r.StatusCodeRange));

        if (rule != null)
            return ShouldSample(rule.SamplingRate);

        return statusCode switch
        {
            >= 500 => ShouldSample(_options.ServerErrorSamplingRate),
            >= 400 => ShouldSample(_options.ClientErrorSamplingRate),
            >= 300 => ShouldSample(_options.RedirectSamplingRate),
            >= 200 => ShouldSample(_options.SuccessSamplingRate),
            _ => ShouldSample(_options.DefaultSamplingRate)
        };
    }

    private bool HasDependencyFailure(Activity activity)
    {
        // First check if this is an outbound dependency call (client activity)
        // Server activities represent incoming requests, not dependencies
        if (activity.Kind != ActivityKind.Client && activity.Kind != ActivityKind.Producer)
        {
            return false;
        }

        // Check activity status - this is the most reliable indicator of failure
        if (activity.Status == ActivityStatusCode.Error)
        {
            return true;
        }

        if (ErrorTags.Select(errorTag => activity.GetTagItem(errorTag)?.ToString())
            .Any(errorValue => !string.IsNullOrEmpty(errorValue)))
        {
            return true;
        }

        // Check HTTP status codes for client calls (any 4xx/5xx indicates failure)
        if (TryGetHttpStatusCode(activity, out var statusCode))
        {
            return statusCode >= 400;
        }

        // Check for common failure indicators in activity names or tags
        var activityName = activity.DisplayName?.ToLowerInvariant() ?? string.Empty;
        var operationName = activity.GetTagItem("operation.name")?.ToString()?.ToLowerInvariant() ?? string.Empty;

        var failureIndicators = new[]
        {
            "timeout", "failed", "error", "exception", "cancelled",
            "abort", "disconnect", "unavailable", "rejected"
        };

        return failureIndicators.Any(indicator => activityName.Contains(indicator) 
                                                  || operationName.Contains(indicator));
    }

    private SamplingDecisionResult ShouldSampleForDependencyFailure()
    {
        return ShouldSample(_options.DependencyFailureSamplingRate);
    }

    private SamplingDecisionResult ShouldSampleForSlowRequest()
    {
        return ShouldSample(_options.SlowRequestSamplingRate);
    }

    private static bool IsInRange(int statusCode, StatusCodeRange? range)
    {
        return range != null && statusCode >= range.Min && statusCode <= range.Max;
    }

    /// <summary>
    /// Performs probabilistic sampling based on the given rate.
    /// For testing purposes, rates of 0.0 always return false and rates of 1.0 always return true.
    /// </summary>
    /// <param name="samplingRate">The sampling rate between 0.0 and 1.0</param>
    /// <returns>True if the item should be sampled, false otherwise</returns>
    private static SamplingDecisionResult ShouldSample(double samplingRate)
    {
        var decision = new SamplingDecisionResult() { SampleRate = samplingRate * 100 };

        switch (samplingRate)
        {
            case <= 0.0:
                decision.ShouldSample = false;
                return decision;
            case >= 1.0:
                decision.ShouldSample = true;
                decision.SampleRate = 100;
                return decision;
            default:
                decision.ShouldSample = Random.Shared.NextDouble() < samplingRate;
                return decision;
        }
    }

    /// <summary>
    /// Releases the resources used by the TailBasedSamplingProcessor.
    /// This method clears all pending spans to prevent memory leaks when the processor is disposed.
    /// </summary>
    /// <param name="disposing">True if the method is being called from the Dispose method; false if being called from the finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pendingSpans.Clear();
        }

        base.Dispose(disposing);
    }
}