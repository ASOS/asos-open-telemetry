using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Asos.OpenTelemetry.AspNetCore.Sampling;

/// <summary>
/// A class representing a sampling rule for route-based sampling.
/// </summary>
public class SamplingRule
{
    /// <summary>
    /// A pattern that matches the route. This can be a regular expression.
    /// </summary>
    public string RoutePattern { get; set; } = string.Empty;
    
    /// <summary>
    /// The HTTP method (e.g., GET, POST) to which this rule applies.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// The sampling rate for this rule. This should be a value between 0.0 and 1.0.
    /// </summary>
    public double Rate { get; set; }
    
    [JsonIgnore]
    public Regex? CompiledPattern { get; set; }
}