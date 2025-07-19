namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// Defines a sampling rule for specific exception types during tail-based sampling.
/// This rule allows you to configure different sampling rates for different types of exceptions,
/// enabling more granular control over which exceptions get captured in traces.
/// </summary>
public class ExceptionRule
{
    /// <summary>
    /// Gets or sets the full type name of the exception to match against.
    /// This should be the complete type name including namespace (e.g., "System.ArgumentNullException").
    /// Matching is performed case-insensitively against the exception.type tag in the activity.
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the sampling rate for this exception type, expressed as a decimal between 0.0 and 1.0.
    /// A value of 1.0 means all spans with this exception type will be sampled,
    /// while 0.0 means none will be sampled. Values between 0 and 1 enable probabilistic sampling.
    /// </summary>
    public double SamplingRate { get; set; }
}
