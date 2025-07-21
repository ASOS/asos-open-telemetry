using Asos.OpenTelemetry.AspNetCore.Sampling.Head;
using Asos.OpenTelemetry.AspNetCore.Sampling.Tail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
    public static TracerProviderBuilder AddCustomSamplingTraceExporter(
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
    /// Extension method to configure OpenTelemetry with custom sampling for traces. Uses the configuration
    /// for route-based sampling defined in the "OpenTelemetry:Sampling" section of the configuration.
    /// </summary>
    /// <param name="builder">A web application builder instance</param>
    public static void AddOpenTelemetryCustomSampling(this WebApplicationBuilder builder)
    {
        var routeSamplingOptions = new RouteSamplingOptions();
        builder.Configuration
            .GetSection("OpenTelemetry:Sampling")
            .Bind(routeSamplingOptions);
        
        AddOpenTelemetryCustomSampling(builder, routeSamplingOptions);
    }
    
    /// <summary>
    /// Extension method to configure OpenTelemetry with custom sampling for traces. Uses the provided
    /// configuration for route-based sampling.
    /// </summary>
    /// <param name="builder">A web application builder instance</param>
    /// <param name="routeSamplingOptions">An instance of options to configure the sampler behaviour</param>
    public static void AddOpenTelemetryCustomSampling(this WebApplicationBuilder builder, RouteSamplingOptions routeSamplingOptions)
    {
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.Configure<RouteSamplingOptions>(options =>
        {
            options.RouteSamplingRules = routeSamplingOptions.RouteSamplingRules;
            options.DefaultRate = routeSamplingOptions.DefaultRate;
            options.RespectSamplingHeader = routeSamplingOptions.RespectSamplingHeader;
        });

        builder.Services.AddSingleton<RouteRuleSampler>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RouteSamplingOptions>>().Value;
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new RouteRuleSampler(options, httpContextAccessor);
        });
        
        // Register the tail-based sampling processor
        builder.Services.AddSingleton<TailBasedSamplingProcessor>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TailSamplingOptions>>().Value;
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new TailBasedSamplingProcessor(options, httpContextAccessor);
        });
        
        builder.Services.ConfigureOpenTelemetryTracerProvider(providerBuilder =>
        {
            providerBuilder
                .AddCustomSamplingTraceExporter()
                .AddProcessor(sp => sp.GetRequiredService<TailBasedSamplingProcessor>());
        });         
    }
}
