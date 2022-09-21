using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

namespace Asos.OpenTelemetry.Exporter.EventHubs;

public static class MeterProviderExtensions
{
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