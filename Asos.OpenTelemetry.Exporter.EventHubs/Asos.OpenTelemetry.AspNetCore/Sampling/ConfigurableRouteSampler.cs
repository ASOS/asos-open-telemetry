using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Sampling;

/// <summary>
/// An implementation of <see cref="Sampler"/> that samples based on the route and method of an HTTP request. Allows
/// you to specify different sampling rates for different routes and methods.
/// </summary>
public class ConfigurableRouteSampler : Sampler
{
    private readonly RouteSamplingOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Default constructor for <see cref="ConfigurableRouteSampler"/>.
    /// </summary>
    /// <param name="options">A <see cref="RouteSamplingOptions"/> instance </param>
    /// <param name="httpContextAccessor">An instance of IHttpContextAccessor</param>
    public ConfigurableRouteSampler(RouteSamplingOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public override SamplingResult ShouldSample(in SamplingParameters parameters)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var route = httpContext?.Request.Path; 
        var method = httpContext?.Request.Method;
        
        if (string.IsNullOrEmpty(route?.Value) || string.IsNullOrEmpty(method)) 
            return RandomSamplingResult(_options.DefaultRate);
        
        var rule = _options.SamplingRules
            .FirstOrDefault(r =>
                string.Equals(r.Method, method, StringComparison.OrdinalIgnoreCase) &&
                r.CompiledPattern?.IsMatch(route) == true
            );

        var rate = rule?.Rate ?? _options.DefaultRate;
        
        return RandomSamplingResult(rate);
    }

    private static SamplingResult RandomSamplingResult(double probability)
    {
        return probability switch
        {
            >= 1.0 => new SamplingResult(SamplingDecision.RecordAndSample),
            
            <= 0.0 => new SamplingResult(SamplingDecision.Drop),
            
            _ => (Random.Shared.NextDouble() < probability)
                ? new SamplingResult(SamplingDecision.RecordAndSample)
                : new SamplingResult(SamplingDecision.Drop)
        };
    }
}