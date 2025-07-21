namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// Defines a sampling rule for HTTP status codes during tail-based sampling.
/// This rule allows you to configure different sampling rates for specific status codes
/// or ranges of status codes, providing fine-grained control over trace sampling based on response outcomes.
/// </summary>
public class StatusCodeRule
{
    private double _samplingRate;

    /// <summary>
    /// Gets or sets a specific HTTP status code to match against.
    /// When set, this rule will apply to requests that result in exactly this status code.
    /// If StatusCodeRange is also specified, the rule applies to spans matching either the specific code or falling within the range.
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Gets or sets a range of HTTP status codes to match against.
    /// When set, this rule will apply to requests with status codes falling within the specified range (inclusive).
    /// This allows you to create rules for categories like "all 4xx errors" or "all 5xx errors".
    /// If StatusCode is also specified, the rule applies to spans matching either the specific code or falling within the range.
    /// </summary>
    public StatusCodeRange? StatusCodeRange { get; set; }
    
    /// <summary>
    /// Gets or sets the sampling rate for matching status codes, expressed as a decimal between 0.0 and 1.0.
    /// A value of 1.0 means all spans with matching status codes will be sampled,
    /// while 0.0 means none will be sampled. Values between 0 and 1 enable probabilistic sampling.
    /// </summary>
    public double SamplingRate 
    { 
        get => _samplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _samplingRate = value;
        }
    }
}
