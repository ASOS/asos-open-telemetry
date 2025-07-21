using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Asos.OpenTelemetry.AspNetCore.Sampling;

/// <summary>
/// A class representing a sampling rule for route-based sampling.
/// </summary>
public class RouteSamplingRule
{
    private string _routePattern = string.Empty;
    private double _rate;

    /// <summary>
    /// A pattern that matches the route. This can be a regular expression.
    /// </summary>
    public string RoutePattern
    {
        get => _routePattern;
        set
        {
            _routePattern = value;
            CompilePattern(); 
        }
    }

    /// <summary>
    /// The HTTP method (e.g., GET, POST) to which this rule applies.
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// The sampling rate for this rule. This should be a value between 0.0 and 1.0.
    /// </summary>
    public double Rate 
    { 
        get => _rate;
        set
        {
            if (value < 0.0 || value > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _rate = value;
        }
    }
    
    /// <summary>
    /// Compiled regular expression for the route pattern, used by the sampling processor.
    /// </summary>
    [JsonIgnore]
    public Regex? CompiledPattern { get; private set; }
    
    private void CompilePattern()
    {
        if (string.IsNullOrWhiteSpace(RoutePattern))
        {
            CompiledPattern = null;
            return;
        }

        try
        {
            CompiledPattern = new Regex(RoutePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"Invalid route pattern: {RoutePattern}", ex);
        }
    }
}