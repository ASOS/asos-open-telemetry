using System.Text.RegularExpressions;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Sampling;

public static class OpenTelemetryExtensions
{
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
            var sampler = sp.GetRequiredService<ConfigurableRouteSampler>();
            providerBuilder.SetSampler(sampler);
        });
    }
    
    public static void ConfigureOpenTelemetry(this WebApplicationBuilder builder, Action<AzureMonitorOptions> configureOptions)
    {
        builder.Services.AddSingleton<RouteSamplingOptions>()
            .Configure<RouteSamplingOptions>(builder.Configuration.GetSection("OpenTelemetry:Sampling"));

        builder.Services.AddSingleton<ConfigurableRouteSampler>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RouteSamplingOptions>>().Value;
            foreach (var rule in options.SamplingRules)
            {
                rule.CompiledPattern = new Regex(rule.RoutePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new ConfigurableRouteSampler(options, httpContextAccessor);
        });
        
        builder.Services.AddOpenTelemetry().UseAzureMonitor(configureOptions);

        builder.Services.ConfigureOpenTelemetryTracerProvider(providerBuilder =>
        {
            providerBuilder.AddCustomSamplingAzureMonitorTraceExporter();
        });               
    }
}

