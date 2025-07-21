namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// Configuration options for tail-based sampling that define sampling rates and rules
/// for different types of span outcomes including HTTP status codes, exceptions, 
/// dependency failures, and request performance characteristics.
/// </summary>
public class TailSamplingOptions
{
    private double _defaultSamplingRate = 0.1;
    private double _defaultExceptionSamplingRate = 1.0;
    private double _serverErrorSamplingRate = 1.0;
    private double _clientErrorSamplingRate = 0.5;
    private double _redirectSamplingRate = 0.1;
    private double _successSamplingRate = 0.05;
    private double _dependencyFailureSamplingRate = 1.0;
    private double _slowRequestSamplingRate = 0.8;

    /// <summary>
    /// Gets or sets the default sampling rate applied when no specific rules match.
    /// This serves as the fallback sampling rate for spans that don't meet any other criteria.
    /// Value should be between 0.0 (no sampling) and 1.0 (sample everything).
    /// </summary>
    public double DefaultSamplingRate 
    { 
        get => _defaultSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _defaultSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the default sampling rate for spans that contain exceptions.
    /// This rate is used when an exception is detected but no specific exception rule matches.
    /// Typically set higher than normal sampling rates to ensure error visibility.
    /// </summary>
    public double DefaultExceptionSamplingRate 
    { 
        get => _defaultExceptionSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _defaultExceptionSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 5xx server error status codes.
    /// These errors typically indicate server-side issues and are usually sampled at high rates
    /// for debugging and monitoring purposes.
    /// </summary>
    public double ServerErrorSamplingRate 
    { 
        get => _serverErrorSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _serverErrorSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 4xx client error status codes.
    /// These errors indicate client-side issues like bad requests or unauthorized access.
    /// Usually sampled at moderate rates to balance visibility with storage costs.
    /// </summary>
    public double ClientErrorSamplingRate 
    { 
        get => _clientErrorSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _clientErrorSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 3xx redirect status codes.
    /// Redirects are typically less critical for debugging and are often sampled at lower rates.
    /// </summary>
    public double RedirectSamplingRate 
    { 
        get => _redirectSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _redirectSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 2xx success status codes.
    /// Successful requests are usually sampled at lower rates since they don't indicate problems,
    /// but some sampling is maintained for performance monitoring and baseline establishment.
    /// </summary>
    public double SuccessSamplingRate 
    { 
        get => _successSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _successSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for spans that represent failed dependency calls.
    /// This includes failed database calls, HTTP client errors, timeouts, and connection issues.
    /// Typically set high to ensure visibility into external service problems.
    /// </summary>
    public double DependencyFailureSamplingRate 
    { 
        get => _dependencyFailureSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _dependencyFailureSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the sampling rate for requests that exceed the slow request threshold.
    /// Slow requests are important for performance monitoring and are usually sampled at high rates
    /// to identify performance bottlenecks and optimization opportunities.
    /// </summary>
    public double SlowRequestSamplingRate 
    { 
        get => _slowRequestSamplingRate;
        set
        {
            if (value is < 0.0 or > 1.0)
                throw new ArgumentException("Sample rate must be between 0.0 and 1.0", nameof(value));
            _slowRequestSamplingRate = value;
        }
    }
    
    /// <summary>
    /// Gets or sets the duration threshold above which a request is considered "slow".
    /// Requests taking longer than this threshold will be evaluated using the SlowRequestSamplingRate.
    /// This helps identify performance issues and long-running operations.
    /// </summary>
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromSeconds(2);
    
    /// <summary>
    /// Gets or sets the list of HTTP status code-specific sampling rules that define custom sampling rates
    /// for specific status codes or ranges of status codes. These rules take precedence over
    /// the general category-based sampling rates (like ServerErrorSamplingRate).
    /// </summary>
    public List<StatusCodeRule> StatusCodeRules { get; set; } = [];
}
