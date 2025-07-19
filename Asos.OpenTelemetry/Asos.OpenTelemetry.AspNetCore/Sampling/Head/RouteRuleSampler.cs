using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Sampling.Head;

/// <summary>
/// An implementation of <see cref="Sampler"/> that samples based on the route and method of an HTTP request. Allows
/// you to specify different sampling rates for different routes and methods.
///
/// This is a Head based sampler, meaning it is applied at the start of the trace. 
/// </summary>
public class RouteRuleSampler : Sampler
{
    private readonly RouteSamplingOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Default constructor for <see cref="RouteRuleSampler"/>.
    /// </summary>
    /// <param name="options">A <see cref="RouteSamplingOptions"/> instance </param>
    /// <param name="httpContextAccessor">An instance of IHttpContextAccessor</param>
    public RouteRuleSampler(RouteSamplingOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Custom sampling logic that determines whether a trace should be sampled based on the HTTP request's route and method.
    ///
    /// This will check the current HTTP context's request path and method against the configured sampling rules, and if
    /// it matches a rule, it will return a sampling decision based on the rate specified in that rule.
    /// </summary>
    /// <param name="parameters">A <see cref="SamplingParameters"/> instance</param>
    /// <returns>A <see cref="SamplingResult"/></returns>
    public override SamplingResult ShouldSample(in SamplingParameters parameters)
    {
        if (_options.RespectSamplingHeader)
        {
            // If we've indicated that we should respect the parent trace then we should check the parent context's trace flags.
            // If we're already sampling this trace, then we should continue to sample it.
            if ((parameters.ParentContext.TraceFlags & ActivityTraceFlags.Recorded) != 0)
            {
                return new SamplingResult(SamplingDecision.RecordAndSample);
            }   
            
            // If we have a parent trace, but it's not sampled, respect that decision
            if (parameters.ParentContext.TraceId != default)
            {
                return new SamplingResult(SamplingDecision.Drop);
            }
        }
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            // The sampler runs very early in the pipeline, and HttpContext might not
            // always be available when the sampler is called.
            return RandomSamplingResult(_options.DefaultRate);
        }
        
        var path = httpContext.Request.Path.Value; 
        var method = httpContext.Request.Method;
        
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(method)) 
            return RandomSamplingResult(_options.DefaultRate);
        
        var rule = _options.RouteSamplingRules
            .FirstOrDefault(r =>
                string.Equals(r.Method, method, StringComparison.OrdinalIgnoreCase) &&
                r.CompiledPattern?.IsMatch(path) == true
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