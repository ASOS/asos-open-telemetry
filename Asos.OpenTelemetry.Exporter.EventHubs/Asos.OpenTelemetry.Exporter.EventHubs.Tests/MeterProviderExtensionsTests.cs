using NUnit.Framework;
using OpenTelemetry;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;


public class MeterProviderExtensionsTests
{
    private const string WellFormedTargetEndpoint = "https://some-host.servicebus.windows.net/EventHubName";

    [Test]
    public void Should_Create_Meter_Builder_Using_Export_Options()
    {
        var eventHubOptions = new EventHubOptions
        {
            AuthenticationMode = AuthenticationMode.SasKey,
            AccessKey = "key",
            EventHubFqdn = WellFormedTargetEndpoint,
            KeyName = "test"
        };

        var builder = Sdk.CreateMeterProviderBuilder()
            .AddOtlpEventHubExporter(eventHubOptions);

        Assert.IsNotNull(builder);
    }
}