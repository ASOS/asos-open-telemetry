# Open Telemetry Export for Event Hubs

A library for sending OTLP data to an Azure Event Hubs endpoint. 

## What's it for?

This library is specifically to simplify in process scenarios where agents or other collector patterns aren't an option 
and you'd like the process being instrumented to be responsible for transmitting data to the target

## How does it work?

This is a bit of syntantic sugar to help with bootstrapping the Event Hubs endpoint and setting up authentication. The exporter
option we expose here `AddOtlpEventHubExporter` builds directly onto `AddOtlpExporter` and sets up the necessary configuration. 

In particular, that's setting the protocol to `HttpProtobuf` and the `HttpClientFactory` to take an instance that handles tokens and 
token refreshes. 

The library will support either SAS key authentication or Managed Identity, and sets up the `HttpClient` to transmit the appropriate
authorization header. 
   
## Example configurations

Create an `EventHubOptions` object and choose from either SAS key authentication or managed identity. When configuring your services, you
now have an extension named `AddOtlpEventHubExporter` that you can pass the options to


```csharp
var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.SasKey,
    KeyName = "the-name-of-the-access-key"    
    AccessKey = "the-event-hub-access-key",
    EventHubFqdn = "fully-qualified-target-eventhub-uri"
};

OR

var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = "fully-qualified-target-eventhub-uri"
};

services.AddOpenTelemetryMetrics(builder => builder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DemoService"))
    .AddAspNetCoreInstrumentation()
    .AddMeter("MeterName")
    .AddOtlpEventHubExporter(eventHubOptions));
```

## Permissions

When running as a SAS key, the permissions are available from the access key you've used. However, when running in ManagedIdentity, you'll need to 
grant the [Azure Event Hubs Data Sender](https://learn.microsoft.com/en-us/azure/event-hubs/authenticate-application) role to the identity you 
want to access the Event Hub endpoint.
