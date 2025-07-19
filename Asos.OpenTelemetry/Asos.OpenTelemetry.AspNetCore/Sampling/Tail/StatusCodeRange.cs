namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// Defines a range of HTTP status codes for use in tail-based sampling rules.
/// This allows you to create sampling rules that apply to ranges of status codes
/// (e.g., all 4xx client errors or all 5xx server errors) rather than individual codes.
/// </summary>
public class StatusCodeRange
{
    /// <summary>
    /// Gets or sets the minimum HTTP status code in the range (inclusive).
    /// For example, setting this to 400 would include status code 400 in the range.
    /// </summary>
    public int Min { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum HTTP status code in the range (inclusive).
    /// For example, setting this to 499 would include status code 499 in the range.
    /// Combined with Min=400, this would cover all 4xx client error status codes.
    /// </summary>
    public int Max { get; set; }
}
