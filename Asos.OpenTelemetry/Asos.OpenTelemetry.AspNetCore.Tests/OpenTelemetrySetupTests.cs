using System.Text.RegularExpressions;
using Asos.OpenTelemetry.AspNetCore.Sampling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Tests;

public class OpenTelemetrySetupTests
{
    [Test]
    public void ConfigureOpenTelemetry_RegistersRequiredServices()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["OpenTelemetry:Sampling:SamplingRules:0:RoutePattern"] = "/api/test";
        
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        builder.ConfigureOpenTelemetry(options =>
        {
            options.SamplingRatio = 0.5f;
            options.ConnectionString = "InstrumentationKey=12345-12345-12345-12345";
        });

        var provider = builder.Services.BuildServiceProvider();
        
        var tracerProvider = provider.GetRequiredService<TracerProvider>();
        Assert.That(tracerProvider, Is.Not.Null);
        
        // Assert RouteSamplingOptions are bound correctly
        var routeSamplingOptions = provider.GetRequiredService<IOptions<RouteSamplingOptions>>().Value;
        Assert.That(routeSamplingOptions.SamplingRules, Has.Exactly(1).Items);
        Assert.Multiple(() =>
        {
            Assert.That(routeSamplingOptions.SamplingRules[0].RoutePattern, Is.EqualTo("/api/test"));
            Assert.That(routeSamplingOptions.SamplingRules[0].CompiledPattern, Is.Not.Null);
        });
        Assert.That(routeSamplingOptions.SamplingRules[0].CompiledPattern, Is.InstanceOf<Regex>());

        // Assert that ConfigurableRouteSampler is registered
        var sampler = provider.GetService<ConfigurableRouteSampler>();
        Assert.That(sampler, Is.Not.Null);
    }
}