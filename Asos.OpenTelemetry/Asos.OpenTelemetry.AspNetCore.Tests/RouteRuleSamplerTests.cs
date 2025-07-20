using System.Diagnostics;
using Asos.OpenTelemetry.AspNetCore.Sampling;
using Asos.OpenTelemetry.AspNetCore.Sampling.Head;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Tests;

[TestFixture]
public class RouteRuleSamplerTests
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
            RouteSamplingRules =
            [
                new RouteSamplingRule
                {
                    RoutePattern = "^/api/test$",
                    Method = "GET",
                    Rate = 1.0,
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

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
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

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_BoundarySamplingRates()
    {
        _options.DefaultRate = 0.0;
        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop));

        _options.DefaultRate = 1.0;
        sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_NullHttpContext()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext)null!);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
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

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
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

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_EmptySamplingRules()
    {
        _options.RouteSamplingRules.Clear();

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_InvalidSamplingRate()
    {
        _options.DefaultRate = -1.0;

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(default);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop));
    }

    [Test]
    public void ShouldSample_Concurrency()
    {
        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);

        Parallel.For(0, 100, _ =>
        {
            var result = sampler.ShouldSample(default);
            Assert.That(result.Decision,
                Is.EqualTo(SamplingDecision.Drop).Or.EqualTo(SamplingDecision.RecordAndSample));
        });
    }

    [Test]
    public void ShouldSample_RespectSamplingHeader_ParentContextRecorded_ShouldRecordAndSample()
    {
        _options.RespectSamplingHeader = true;

        var parentContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        var parameters = new SamplingParameters(
            parentContext,
            ActivityTraceId.CreateRandom(),
            "test-operation",
            ActivityKind.Internal);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(parameters);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_RespectSamplingHeader_ParentContextNotRecorded_ShouldDrop()
    {
        _options.RespectSamplingHeader = true;

        var parentContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.None);

        var parameters = new SamplingParameters(
            parentContext,
            ActivityTraceId.CreateRandom(),
            "test-operation",
            ActivityKind.Internal);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(parameters);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.Drop));
    }

    [Test]
    public void ShouldSample_RespectSamplingHeader_NoParentTrace_ShouldUseRouteSampling()
    {
        _options.RespectSamplingHeader = true;

        var parentContext = new ActivityContext(
            default,
            default,
            ActivityTraceFlags.None);

        var parameters = new SamplingParameters(
            parentContext,
            ActivityTraceId.CreateRandom(),
            "test-operation",
            ActivityKind.Internal);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(parameters);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }

    [Test]
    public void ShouldSample_RespectSamplingHeaderDisabled_ShouldIgnoreParentContext()
    {
        _options.RespectSamplingHeader = false;

        var parentContext = new ActivityContext(
            ActivityTraceId.CreateRandom(),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);

        var parameters = new SamplingParameters(
            parentContext,
            ActivityTraceId.CreateRandom(),
            "test-operation",
            ActivityKind.Internal);

        var httpContext = new DefaultHttpContext
        {
            Request = { Path = "/api/test", Method = "GET" }
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var sampler = new RouteRuleSampler(_options, _httpContextAccessor);
        var result = sampler.ShouldSample(parameters);

        Assert.That(result.Decision, Is.EqualTo(SamplingDecision.RecordAndSample));
    }
}