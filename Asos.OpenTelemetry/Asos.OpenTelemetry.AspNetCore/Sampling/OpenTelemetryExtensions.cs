using System.Text.RegularExpressions;
using Asos.OpenTelemetry.AspNetCore.Sampling.Head;
using Asos.OpenTelemetry.AspNetCore.Sampling.Tail;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Sampling;

/// <summary>
/// Extensions for configuring OpenTelemetry with custom sampling for Azure Monitor trace exporter.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Configures the OpenTelemetry TracerProviderBuilder to use a custom sampling strategy for Azure Monitor trace exporter.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public static TracerProviderBuilder AddCustomSamplingAzureMonitorTraceExporter(
        this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder is not IDeferredTracerProviderBuilder deferredBuilder)
        {
            throw new InvalidOperationException("The provided TracerProviderBuilder does not implement IDeferredTracerProviderBuilder.");
        }

        return deferredBuilder.Configure((sp, providerBuilder) =>
        {
            var sampler = sp.GetRequiredService<RouteRuleSampler>();
            providerBuilder.SetSampler(sampler);
        });
    }
    
    /// <summary>
    /// Extension method to configure OpenTelemetry with custom sampling for Azure Monitor trace exporter.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    public static void ConfigureOpenTelemetryCustomSampling(this WebApplicationBuilder builder, Action<AzureMonitorOptions> configureOptions)
    {
        builder.Services.AddSingleton<RouteSamplingOptions>()
            .Configure<RouteSamplingOptions>(builder.Configuration.GetSection("OpenTelemetry:Sampling"));

        builder.Services.AddSingleton<RouteRuleSampler>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RouteSamplingOptions>>().Value;
            foreach (var rule in options.RouteSamplingRules)
            {
                rule.CompiledPattern = new Regex(rule.RoutePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new RouteRuleSampler(options, httpContextAccessor);
        });
        
        // Register the tail-based sampling processor
        builder.Services.AddSingleton<TailBasedSamplingProcessor>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TailSamplingOptions>>().Value;
            foreach (var rule in options.RouteSamplingRules)
            {
                rule.CompiledPattern = new Regex(rule.RoutePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new TailBasedSamplingProcessor(options, httpContextAccessor);
        });
        
        builder.Services.AddOpenTelemetry().UseAzureMonitor(configureOptions);

        builder.Services.ConfigureOpenTelemetryTracerProvider(providerBuilder =>
        {
            providerBuilder
                .AddCustomSamplingAzureMonitorTraceExporter()
                .AddProcessor(sp => sp.GetRequiredService<TailBasedSamplingProcessor>());
        });               
    }
}

