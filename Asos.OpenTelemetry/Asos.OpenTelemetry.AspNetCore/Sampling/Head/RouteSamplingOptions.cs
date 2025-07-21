namespace Asos.OpenTelemetry.AspNetCore.Sampling.Head;

/// <summary>
/// Defines options for route-based sampling in OpenTelemetry.
/// </summary>
public class RouteSamplingOptions
{
    private double _defaultRate = 0.05;

    /// <summary>
    /// A list of sampling rules that define the sampling rate for specific routes.
    /// </summary>
    public List<RouteSamplingRule> RouteSamplingRules { get; set; } = [];
    
    /// <summary>
    /// The default rate for sampling if no rules match.
    /// </summary>
    public double DefaultRate 
    { 
        get => _defaultRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _defaultRate = value;
        }
    }
    
    /// <summary>
    /// If true, the sampling header will be respected when determining whether to sample a request. This
    /// allows for external control of sampling decisions via headers and will attempt to keep the
    /// entire request trace consistent with the sampling decision made by the request initiator.
    /// </summary>
    public bool RespectSamplingHeader { get; set; } = true;
}