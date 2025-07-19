using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

namespace Asos.OpenTelemetry.Exporter.EventHubs;

/// <summary>
/// Extensions class for adding the exporter to the Open Telemetry configuration
/// </summary>
public static class MeterProviderExtensions
{
    /// <summary>
    /// Adds <see cref="OtlpMetricExporter"/> to the <see cref="MeterProviderBuilder"/> using
    /// the <see cref="EventHubOptions"/> specified. Configures exporting OTLP to Event Hubs
    /// using Protobuf
    /// </summary>
    /// <param name="builder">The instance of <see cref="MeterProviderBuilder"/> to chain the calls</param>
    /// <param name="options">The instance of <see cref="EventHubOptions"/> used to configure the Event Hub target</param>
    /// <returns>The instance of <see cref="MeterProviderBuilder"/></returns>
    public static MeterProviderBuilder AddOtlpEventHubExporter(
        this MeterProviderBuilder builder, EventHubOptions options)
    {
        options.Validate();

        return builder.AddOtlpExporter((exporterConfig, readerConfig) =>
        {
            exporterConfig.Protocol = OtlpExportProtocol.HttpProtobuf;
            exporterConfig.Endpoint = new Uri($"{options.EventHubFqdn}/messages");

            exporterConfig.HttpClientFactory = () => new HttpClient(
                new AuthenticationDelegatingHandler(
                    new HttpClientHandler(),
                    new TokenCache(options,
                        TokenResolver.ResolveTokenProvider(options)), options));

            readerConfig.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
            {
                ExportIntervalMilliseconds = options.ExportIntervalMilliseconds
            };
        });
    }
}