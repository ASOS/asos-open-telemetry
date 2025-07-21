# 🔄 Asos.OpenTelemetry.Exporter.EventHubs

[![NuGet](https://img.shields.io/nuget/v/Asos.OpenTelemetry.Exporter.EventHubs)](https://www.nuget.org/packages/Asos.OpenTelemetry.Exporter.EventHubs/)
[![Downloads](https://img.shields.io/nuget/dt/Asos.OpenTelemetry.Exporter.EventHubs)](https://www.nuget.org/packages/Asos.OpenTelemetry.Exporter.EventHubs/)

High-performance OpenTelemetry exporter for Azure Event Hubs, enabling direct streaming of OTLP telemetry data to Azure Event Hubs with enterprise-grade authentication and reliability. Perfect for custom telemetry pipelines, data lake ingestion, and multi-tenant observability architectures.

## ✨ Features

- **🚀 Direct EventHubs Streaming**: Stream telemetry data directly to Azure Event Hubs
- **🔐 Enterprise Authentication**: SAS key and Managed Identity support with automatic token refresh
- **⚡ High Performance**: Optimized HttpProtobuf serialization with connection pooling
- **🔄 Automatic Token Management**: Built-in token caching and renewal
- **🛡️ Production Ready**: Comprehensive error handling and retry mechanisms
- **📊 Multiple Telemetry Types**: Support for traces, metrics, and logs
- **🎛️ Flexible Configuration**: Easy integration with existing OpenTelemetry setups

## 📦 Installation

```bash
dotnet add package Asos.OpenTelemetry.Exporter.EventHubs
```

## 🚀 Quick Start

### Managed Identity (Recommended)

```csharp
using Asos.OpenTelemetry.Exporter.EventHubs;

var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = "your-namespace.servicebus.windows.net/your-hub"
};

// For metrics
services.AddOpenTelemetryMetrics(builder => builder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
    .AddAspNetCoreInstrumentation()
    .AddRuntimeInstrumentation()
    .AddOtlpEventHubExporter(eventHubOptions));

// For traces  
services.AddOpenTelemetryTracing(builder => builder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
    .AddAspNetCoreInstrumentation()
    .AddOtlpEventHubExporter(eventHubOptions));
```

### SAS Key Authentication

```csharp
var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.SasKey,
    KeyName = "RootManageSharedAccessKey",
    AccessKey = "your-shared-access-key-here",
    EventHubFqdn = "your-namespace.servicebus.windows.net/your-hub"
};

services.AddOpenTelemetryMetrics(builder => builder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
    .AddAspNetCoreInstrumentation()
    .AddOtlpEventHubExporter(eventHubOptions));
```

## 🏗️ Complete Examples

### ASP.NET Core Web Application

```csharp
using Asos.OpenTelemetry.Exporter.EventHubs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Configure Event Hub options
var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = builder.Configuration.GetValue<string>("EventHubs:TelemetryEndpoint")!
};

// Add OpenTelemetry with Event Hubs export
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(builder.Environment.ApplicationName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["environment"] = builder.Environment.EnvironmentName,
                ["version"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
            }))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpEventHubExporter(eventHubOptions))
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(builder.Environment.ApplicationName))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpEventHubExporter(eventHubOptions));

var app = builder.Build();
app.Run();
```

### Background Service / Worker

```csharp
using Asos.OpenTelemetry.Exporter.EventHubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    var eventHubOptions = new EventHubOptions
    {
        AuthenticationMode = AuthenticationMode.ManagedIdentity,
        EventHubFqdn = context.Configuration.GetValue<string>("EventHubs:TelemetryEndpoint")!
    };

    services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("BackgroundProcessor"))
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddMeter("BackgroundProcessor.Metrics")
            .AddOtlpEventHubExporter(eventHubOptions))
        .WithTracing(tracing => tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("BackgroundProcessor"))
            .AddSource("BackgroundProcessor.Traces")
            .AddOtlpEventHubExporter(eventHubOptions));

    services.AddHostedService<Worker>();
});

var host = builder.Build();
await host.RunAsync();
```

### Console Application

```csharp
using Asos.OpenTelemetry.Exporter.EventHubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;

var services = new ServiceCollection();

var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.SasKey,
    KeyName = Environment.GetEnvironmentVariable("EVENTHUB_KEY_NAME")!,
    AccessKey = Environment.GetEnvironmentVariable("EVENTHUB_ACCESS_KEY")!,
    EventHubFqdn = Environment.GetEnvironmentVariable("EVENTHUB_FQDN")!
};

services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("ConsoleApp"))
        .AddMeter("ConsoleApp.Metrics")
        .AddOtlpEventHubExporter(eventHubOptions));

var serviceProvider = services.BuildServiceProvider();

// Your application logic here
Console.WriteLine("Telemetry streaming to Event Hubs...");
await Task.Delay(5000);

serviceProvider.Dispose();
```

## 🔐 Authentication & Permissions

### Managed Identity Setup

#### Using Azure CLI
```bash
# Create a managed identity
az identity create --name myapp-identity --resource-group myResourceGroup

# Get the principal ID
PRINCIPAL_ID=$(az identity show --name myapp-identity --resource-group myResourceGroup --query principalId -o tsv)

# Assign Event Hubs Data Sender role
az role assignment create \
    --assignee $PRINCIPAL_ID \
    --role "Azure Event Hubs Data Sender" \
    --scope /subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.EventHub/namespaces/{namespace-name}

# For App Service or Container Apps, assign the identity
az webapp identity assign --name myapp --resource-group myResourceGroup --identities /subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/myapp-identity
```

#### Using Azure PowerShell
```powershell
# Create managed identity
$identity = New-AzUserAssignedIdentity -ResourceGroupName "myResourceGroup" -Name "myapp-identity"

# Assign Event Hubs Data Sender role
New-AzRoleAssignment -ObjectId $identity.PrincipalId `
    -RoleDefinitionName "Azure Event Hubs Data Sender" `
    -Scope "/subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.EventHub/namespaces/{namespace-name}"
```

### SAS Key Setup

```bash
# Get connection string from Event Hub
az eventhubs eventhub authorization-rule keys list \
    --resource-group myResourceGroup \
    --namespace-name myNamespace \
    --eventhub-name myHub \
    --name RootManageSharedAccessKey
```

## ⚙️ Configuration Options

### EventHubOptions Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `EventHubFqdn` | `string` | ✅ | Fully qualified domain name of the Event Hub endpoint |
| `AuthenticationMode` | `AuthenticationMode` | ✅ | Authentication method (`SasKey` or `ManagedIdentity`) |
| `KeyName` | `string` | ⚠️* | SAS key name (required for SAS authentication) |
| `AccessKey` | `string` | ⚠️* | SAS access key (required for SAS authentication) |
| `TokenCacheDurationMinutes` | `int` | ❌ | Token cache duration in minutes (default: 50) |

\* Required only when using `AuthenticationMode.SasKey`

### Authentication Modes Comparison

| Feature | SAS Key | Managed Identity |
|---------|---------|------------------|
| **Security** | ⚠️ Key rotation required | ✅ Azure-managed |
| **Setup Complexity** | ✅ Simple | ⚠️ Role assignments needed |
| **Local Development** | ✅ Easy testing | ⚠️ Requires Azure auth |
| **Production** | ⚠️ Key management | ✅ Recommended |
| **Audit Trail** | ⚠️ Limited | ✅ Full Azure AD logs |

### Configuration via appsettings.json

```json
{
  "EventHubs": {
    "TelemetryEndpoint": "telemetry-namespace.servicebus.windows.net/telemetry-hub",
    "AuthenticationMode": "ManagedIdentity"
  },
  "Logging": {
    "LogLevel": {
      "Asos.OpenTelemetry.Exporter.EventHubs": "Information"
    }
  }
}
```

With configuration binding:
```csharp
var eventHubOptions = new EventHubOptions();
builder.Configuration.GetSection("EventHubs").Bind(eventHubOptions);
```

## 🏗️ Architecture & Data Flow

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Application   │    │  OTLP Exporter   │    │  Azure Event    │
│   Telemetry     ├───►│  (HttpProtobuf)  ├───►│     Hubs        │
│  (Traces/Metrics)│    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │                        │
                                ▼                        ▼
                       ┌──────────────────┐    ┌─────────────────┐
                       │ Authentication   │    │   Downstream    │
                       │ Token Manager    │    │  Consumers      │
                       │  (SAS/Managed)   │    │ (Stream Analytics│
                       └──────────────────┘    │  Data Factory)  │
                                               └─────────────────┘
```

## 🚨 Troubleshooting

### Common Issues & Solutions

#### Authentication Failures

**Issue**: `401 Unauthorized` errors
```bash
# Check role assignments
az role assignment list --assignee {principal-id} --all

# Verify Event Hub exists
az eventhubs eventhub show --name {hub-name} --namespace-name {namespace}
```

**Solution**:
```csharp
// Enable detailed logging
builder.Logging.AddFilter("Asos.OpenTelemetry.Exporter.EventHubs", LogLevel.Debug);
```

#### Connection Issues

**Issue**: `ServiceUnavailable` or timeout errors
```csharp
// Configure retry options
services.Configure<EventHubOptions>(options =>
{
    options.TokenCacheDurationMinutes = 30; // Reduce cache duration
});

// Add custom HttpClient configuration
services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
});
```

#### Token Expiration

**Issue**: Intermittent `401` errors after running for extended periods
```csharp
// Monitor token refresh
builder.Logging.AddFilter("Asos.OpenTelemetry.Exporter.EventHubs.Tokens", LogLevel.Information);
```

### Performance Optimization

#### High-Throughput Scenarios

```csharp
// Optimize batch settings
services.AddOpenTelemetryMetrics(metrics => metrics
    .AddOtlpEventHubExporter(eventHubOptions, otlpOptions =>
    {
        otlpOptions.BatchExportProcessorOptions.MaxExportBatchSize = 512;
        otlpOptions.BatchExportProcessorOptions.ExportTimeoutMilliseconds = 10000;
        otlpOptions.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 2000;
    }));
```

#### Memory Management

```csharp
// Configure bounded memory usage
services.Configure<EventHubOptions>(options =>
{
    options.TokenCacheDurationMinutes = 45; // Balance between performance and memory
});
```

## 📊 Monitoring & Observability

### Built-in Metrics

The exporter exposes internal metrics for monitoring:

```csharp
services.AddOpenTelemetryMetrics(metrics => metrics
    .AddMeter("Asos.OpenTelemetry.Exporter.EventHubs") // Internal exporter metrics
    .AddYourApplicationMeters());
```

Available metrics:
- `eventhubs.export.duration` - Export operation duration
- `eventhubs.export.batch_size` - Exported batch sizes
- `eventhubs.auth.token_refresh` - Token refresh operations
- `eventhubs.export.errors` - Export error counts by type

### Health Checks

```csharp
services.AddHealthChecks()
    .AddEventHubsExporter("EventHubsExporter", eventHubOptions);
```

### Application Insights Integration

```csharp
// Dual export to both Event Hubs and Application Insights
services.AddOpenTelemetryTracing(tracing => tracing
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
    .AddAspNetCoreInstrumentation()
    .AddOtlpEventHubExporter(eventHubOptions)  // Custom pipeline
    .AddApplicationInsightsTraceExporter());   // Standard monitoring
```

## 🔧 Advanced Usage

### Custom Event Hub Configuration

```csharp
services.Configure<EventHubOptions>(options =>
{
    options.EventHubFqdn = "custom-namespace.servicebus.windows.net/telemetry-hub";
    options.AuthenticationMode = AuthenticationMode.ManagedIdentity;
    options.TokenCacheDurationMinutes = 45;
    
    // Custom properties for downstream processing
    options.CustomProperties = new Dictionary<string, string>
    {
        ["environment"] = "production",
        ["region"] = "westus2",
        ["version"] = "1.2.3"
    };
});
```

### Multi-tenant Scenarios

```csharp
// Route different tenants to different Event Hubs
services.AddKeyedSingleton("tenant-a", (sp, key) => new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = "tenant-a-namespace.servicebus.windows.net/telemetry"
});

services.AddKeyedSingleton("tenant-b", (sp, key) => new EventHubOptions  
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = "tenant-b-namespace.servicebus.windows.net/telemetry"
});
```

### Integration with Stream Analytics

Event Hubs data can be consumed by Azure Stream Analytics for real-time processing:

```sql
-- Stream Analytics Query Example
SELECT 
    ResourceAttributes.['service.name'] as ServiceName,
    SpanName,
    Duration,
    StatusCode,
    System.Timestamp() as ProcessedTime
FROM TelemetryInput
WHERE StatusCode >= 400
```

## 🎯 Use Cases & Patterns

### Data Lake Ingestion
Stream all telemetry to Event Hubs → Azure Stream Analytics → Azure Data Lake for long-term analytics

### Real-time Alerting  
Stream critical telemetry → Event Hubs → Azure Functions → Custom alerting logic

### Multi-Region Aggregation
Multiple regions → Regional Event Hubs → Central processing → Global dashboards

### Compliance & Audit
All telemetry → Event Hubs → Compliant storage with data sovereignty requirements

---

## 📞 Support & Contributing

- **Issues**: [GitHub Issues](https://github.com/ASOS/asos-open-telemetry/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ASOS/asos-open-telemetry/discussions)
- **Contributing**: See our [Contributing Guide](.github/CONTRIBUTING.md)

Built with ❤️ by ASOS Engineering - powering observability for millions of requests daily.
