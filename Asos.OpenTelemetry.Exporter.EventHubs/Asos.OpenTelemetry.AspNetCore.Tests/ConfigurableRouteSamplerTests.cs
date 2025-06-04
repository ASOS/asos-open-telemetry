using System.Text.RegularExpressions;
using Asos.OpenTelemetry.AspNetCore.Sampling;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Tests;

[TestFixture]
public class ConfigurableRouteSamplerTests
{
    private IHttpContextAccessor _httpContextAccessor;
    private RouteSamplingOptions _options;

    [SetUp]
    public void Setup()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _options = new RouteSamplingOptions
        {
            DefaultRate = 0.5,
            SamplingRules =
            [
                new SamplingRule
                {
                    RoutePattern = "^/api/test$",
                    Method = "GET",
                    Rate = 1.0,
                    CompiledPattern = new Regex("^/api/test$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
                }
            ]
        };
    }

    [Test]
    public void ShouldSample_DefaultSamplingRate_WhenNoMatchingRouteOrMethod()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/unknown", Method = "POST" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_SpecificRouteAndMethodMatch()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_BoundarySamplingRates()
    {
        _options.DefaultRate = 0.0;
        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop));

        _options.DefaultRate = 1.0;
        sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_NullHttpContext()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext)null);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_CaseInsensitiveMethodMatching()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "get" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_RoutePatternMatching()
    {
        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_EmptySamplingRules()
    {
        _options.SamplingRules.Clear();

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_InvalidSamplingRate()
    {
        _options.DefaultRate = -1.0;

        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop));
    }

    [Test]
    public void ShouldSample_Concurrency()
    {
        var sampler = new ConfigurableRouteSampler(_options, _httpContextAccessor);

        Parallel.For(0, 100, _ =>
        {
            var result = sampler.ShouldSample(default);
            Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
        });
    }
}