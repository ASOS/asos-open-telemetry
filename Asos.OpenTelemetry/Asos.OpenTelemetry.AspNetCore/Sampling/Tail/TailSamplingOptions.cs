namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

/// <summary>
/// Configuration options for tail-based sampling that define sampling rates and rules
/// for different types of span outcomes including HTTP status codes, exceptions, 
/// dependency failures, and request performance characteristics.
/// </summary>
public class TailSamplingOptions
{
    /// <summary>
    /// Gets or sets the default sampling rate applied when no specific rules match.
    /// This serves as the fallback sampling rate for spans that don't meet any other criteria.
    /// Value should be between 0.0 (no sampling) and 1.0 (sample everything).
    /// </summary>
    public double DefaultSamplingRate { get; set; } = 0.1;
    
    /// <summary>
    /// Gets or sets the default sampling rate for spans that contain exceptions.
    /// This rate is used when an exception is detected but no specific exception rule matches.
    /// Typically set higher than normal sampling rates to ensure error visibility.
    /// </summary>
    public double DefaultExceptionSamplingRate { get; set; } = 1.0;
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 5xx server error status codes.
    /// These errors typically indicate server-side issues and are usually sampled at high rates
    /// for debugging and monitoring purposes.
    /// </summary>
    public double ServerErrorSamplingRate { get; set; } = 1.0;
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 4xx client error status codes.
    /// These errors indicate client-side issues like bad requests or unauthorized access.
    /// Usually sampled at moderate rates to balance visibility with storage costs.
    /// </summary>
    public double ClientErrorSamplingRate { get; set; } = 0.5;
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 3xx redirect status codes.
    /// Redirects are typically less critical for debugging and are often sampled at lower rates.
    /// </summary>
    public double RedirectSamplingRate { get; set; } = 0.1;
    
    /// <summary>
    /// Gets or sets the sampling rate for HTTP responses with 2xx success status codes.
    /// Successful requests are usually sampled at lower rates since they don't indicate problems,
    /// but some sampling is maintained for performance monitoring and baseline establishment.
    /// </summary>
    public double SuccessSamplingRate { get; set; } = 0.05;
    
    /// <summary>
    /// Gets or sets the sampling rate for spans that represent failed dependency calls.
    /// This includes failed database calls, HTTP client errors, timeouts, and connection issues.
    /// Typically set high to ensure visibility into external service problems.
    /// </summary>
    public double DependencyFailureSamplingRate { get; set; } = 1.0;
    
    /// <summary>
    /// Gets or sets the sampling rate for requests that exceed the slow request threshold.
    /// Slow requests are important for performance monitoring and are usually sampled at high rates
    /// to identify performance bottlenecks and optimization opportunities.
    /// </summary>
    public double SlowRequestSamplingRate { get; set; } = 0.8;
    
    /// <summary>
    /// Gets or sets the duration threshold above which a request is considered "slow".
    /// Requests taking longer than this threshold will be evaluated using the SlowRequestSamplingRate.
    /// This helps identify performance issues and long-running operations.
    /// </summary>
    public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromSeconds(2);
    
    /// <summary>
    /// Gets or sets the list of route-specific sampling rules that define custom sampling rates
    /// for specific HTTP routes and methods. These rules allow fine-grained control over
    /// sampling based on the request path and HTTP method patterns.
    /// </summary>
    public List<RouteSamplingRule> RouteSamplingRules { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the list of exception-specific sampling rules that define custom sampling rates
    /// for different types of exceptions. This allows you to apply different sampling strategies
    /// based on the specific exception types encountered in your application.
    /// </summary>
    public List<ExceptionRule> ExceptionRules { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the list of HTTP status code-specific sampling rules that define custom sampling rates
    /// for specific status codes or ranges of status codes. These rules take precedence over
    /// the general category-based sampling rates (like ServerErrorSamplingRate).
    /// </summary>
    public List<StatusCodeRule> StatusCodeRules { get; set; } = [];
}
