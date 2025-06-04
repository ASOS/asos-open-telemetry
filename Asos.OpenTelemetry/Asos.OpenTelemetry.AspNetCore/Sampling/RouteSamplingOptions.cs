namespace Asos.OpenTelemetry.AspNetCore.Sampling;

/// <summary>
/// Defines options for route-based sampling in OpenTelemetry.
/// </summary>
public class RouteSamplingOptions
{
    /// <summary>
    /// A list of sampling rules that define the sampling rate for specific routes.
    /// </summary>
    public List<SamplingRule> SamplingRules { get; set; } = new();
    
    /// <summary>
    /// The default rate for sampling if no rules match.
    /// </summary>
    public double DefaultRate { get; set; } = 0.05;
}