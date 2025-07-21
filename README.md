# 🚀 ASOS OpenTelemetry Extensions

[![License](https://img.shields.io/github/license/ASOS/asos-open-telemetry)](LICENSE)
[![Build Status](https://dev.azure.com/asos/asos-open-telemetry/_apis/build/status/main?branchName=main)](https://dev.azure.com/asos/asos-open-telemetry/_build)

A comprehensive collection of OpenTelemetry extensions and contributions specifically designed to enhance observability in .NET applications. This repository contains enterprise-ready libraries that provide advanced sampling strategies, efficient data export mechanisms, and seamless Azure integration.

## 📦 Packages

### 🎯 [Asos.OpenTelemetry.AspNetCore](./Asos.OpenTelemetry/Asos.OpenTelemetry.AspNetCore)
Advanced sampling strategies for ASP.NET Core applications with Azure Monitor integration.

**Key Features:**
- **Head-based Sampling**: Route-specific sampling decisions at trace start
- **Tail-based Sampling**: Outcome-driven sampling based on status codes, exceptions, and dependencies  
- **Regex Route Patterns**: Flexible route matching with compiled regex performance
- **Azure Monitor Integration**: Seamless integration with Azure Application Insights
- **Performance Optimized**: Minimal overhead with intelligent caching

**Perfect for:** High-traffic web applications requiring intelligent trace sampling to manage costs and reduce noise while preserving critical observability data.

### 🔄 [Asos.OpenTelemetry.Exporter.EventHubs](./Asos.OpenTelemetry/Asos.OpenTelemetry.Exporter.EventHubs)  
High-performance OTLP data export to Azure Event Hubs with enterprise authentication support.

**Key Features:**
- **Direct EventHubs Export**: Stream telemetry data directly to Azure Event Hubs
- **Multiple Authentication Modes**: SAS keys and Managed Identity support
- **Automatic Token Management**: Built-in token refresh and caching
- **Enterprise Ready**: Production-tested with comprehensive error handling
- **Protocol Optimization**: Efficient HttpProtobuf serialization

**Perfect for:** Enterprise environments requiring custom telemetry pipelines, data lake ingestion, or multi-tenant observability architectures.

## 🚀 Quick Start

### ASP.NET Core with Intelligent Sampling

```csharp
using Asos.OpenTelemetry.AspNetCore.Sampling;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry with custom sampling
builder.ConfigureOpenTelemetryCustomSampling(options =>
{
    options.ConnectionString = "InstrumentationKey=your-key;IngestionEndpoint=https://...";
});

var app = builder.Build();
app.Run();
```

### Event Hubs Export

```csharp
using Asos.OpenTelemetry.Exporter.EventHubs;

var eventHubOptions = new EventHubOptions
{
    AuthenticationMode = AuthenticationMode.ManagedIdentity,
    EventHubFqdn = "your-namespace.servicebus.windows.net/your-hub"
};

services.AddOpenTelemetryMetrics(builder => builder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
    .AddAspNetCoreInstrumentation()
    .AddOtlpEventHubExporter(eventHubOptions));
```

## 🎯 Use Cases

### 🏪 **E-Commerce Platforms**
- Sample health checks at 1%, order processing at 100%
- Capture all payment failures while ignoring successful product browsing
- Route-specific sampling for different user journeys

### 🌐 **High-Traffic APIs**  
- Intelligent sampling based on endpoint criticality
- Exception-driven sampling to capture all errors
- Dependency failure detection with automatic sampling adjustment

### 🏢 **Enterprise Microservices**
- Custom telemetry pipelines via Event Hubs
- Multi-tenant data isolation and routing
- Compliance-ready data export with Azure integration

### 📊 **Data Analytics Platforms**
- Stream telemetry to data lakes via Event Hubs  
- Real-time observability dashboards
- Cost-optimized sampling strategies

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────────┐    ┌─────────────────┐
│  ASP.NET Core   │    │   Sampling Engine    │    │  Azure Monitor  │
│   Application   ├───►│ Head + Tail Based    ├───►│ Application     │
│                 │    │   Route Matching     │    │   Insights      │
└─────────────────┘    └──────────────────────┘    └─────────────────┘
                                  │
                                  ▼
                       ┌──────────────────────┐    ┌─────────────────┐
                       │    Event Hubs        │    │   Custom Data   │
                       │     Exporter         ├───►│   Pipeline      │
                       │  SAS + Managed ID    │    │  (Data Lake)    │
                       └──────────────────────┘    └─────────────────┘
```

## 🔧 Configuration

Both packages support comprehensive configuration through `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "Sampling": {
      "DefaultRate": 0.05,
      "RespectSamplingHeader": true,
      "RouteSamplingRules": [
        {
          "RoutePattern": "^/health$",
          "Method": "GET", 
          "Rate": 0.01
        },
        {
          "RoutePattern": "^/api/orders$",
          "Method": "POST",
          "Rate": 1.0
        }
      ],
      "TailSampling": {
        "MaxSpanCount": 10000,
        "DecisionWaitTimeMs": 5000,
        "StatusCodeRules": [
          {
            "StatusCodeRanges": ["400-499", "500-599"],
            "Rate": 1.0
          }
        ],
        "ExceptionRules": [
          {
            "ExceptionType": "System.Exception",
            "Rate": 1.0
          }
        ]
      }
    }
  }
}
```

## 📚 Documentation

- **[AspNetCore Package](./Asos.OpenTelemetry/Asos.OpenTelemetry.AspNetCore/README.md)**: Comprehensive sampling documentation with real-world examples
- **[EventHubs Exporter](./Asos.OpenTelemetry/Asos.OpenTelemetry.Exporter.EventHubs/README.md)**: Complete setup guide with authentication examples

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](.github/CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏢 About ASOS

Built with ❤️ by the ASOS engineering team. These libraries power observability for one of the world's largest online fashion retailers, handling millions of requests daily with intelligent sampling and reliable data export.

