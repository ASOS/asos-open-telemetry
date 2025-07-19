# 🎯 Asos.OpenTelemetry.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/Asos.OpenTelemetry.AspNetCore)](https://www.nuget.org/packages/Asos.OpenTelemetry.AspNetCore/)
[![Downloads](https://img.shields.io/nuget/dt/Asos.OpenTelemetry.AspNetCore)](https://www.nuget.org/packages/Asos.OpenTelemetry.AspNetCore/)

Advanced OpenTelemetry sampling strategies for ASP.NET Core applications with seamless Azure Monitor integration. This package provides intelligent, configurable sampling mechanisms that help manage observability costs while preserving critical trace data.

## ✨ Features

- **🎲 Head-based Sampling**: Make sampling decisions at trace initiation based on route patterns
- **🔍 Tail-based Sampling**: Make sampling decisions after spans complete based on outcomes  
- **📍 Route-based Rules**: Regex pattern matching for flexible route-specific sampling
- **⚡ High Performance**: Compiled regex patterns with efficient caching
- **🔗 Azure Integration**: Built-in Azure Monitor and Application Insights support
- **🎛️ Flexible Configuration**: JSON-based configuration with runtime updates
- **🛡️ Production Ready**: Battle-tested in high-traffic environments

## 📦 Installation

```bash
dotnet add package Asos.OpenTelemetry.AspNetCore
```

## 🚀 Quick Start

### Basic Setup

```csharp
using Asos.OpenTelemetry.AspNetCore.Sampling;

var builder = WebApplication.CreateBuilder(args);

// Add required services
builder.Services.AddHttpContextAccessor();

// Configure OpenTelemetry with custom sampling
builder.ConfigureOpenTelemetryCustomSampling(options =>
{
    options.ConnectionString = "InstrumentationKey=your-key;IngestionEndpoint=https://...";
});

var app = builder.Build();
app.Run();
```

### Configuration (appsettings.json)

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
      ]
    }
  }
}
```

## 🎲 Head-based Sampling

Head-based sampling makes decisions at the start of a trace based on the HTTP request properties. This is efficient and prevents unnecessary processing.

### Route Rule Configuration

```json
{
  "OpenTelemetry": {
    "Sampling": {
      "DefaultRate": 0.1,
      "RespectSamplingHeader": true,
      "RouteSamplingRules": [
        {
          "RoutePattern": "^/health$",
          "Method": "GET",
          "Rate": 0.0
        },
        {
          "RoutePattern": "^/api/users/\\d+$",
          "Method": "GET", 
          "Rate": 0.25
        },
        {
          "RoutePattern": "^/api/orders$",
          "Method": "POST",
          "Rate": 1.0
        },
        {
          "RoutePattern": "^/api/payments/.*$",
          "Method": "*",
          "Rate": 1.0
        }
      ]
    }
  }
}
```

### Advanced Route Patterns

```json
{
  "RouteSamplingRules": [
    {
      "RoutePattern": "^/api/products(?:/\\d+)?(?:/reviews)?$",
      "Method": "GET",
      "Rate": 0.05,
      "Description": "Low sampling for product browsing"
    },
    {
      "RoutePattern": "^/api/checkout/.*$", 
      "Method": "*",
      "Rate": 1.0,
      "Description": "Always sample checkout flow"
    },
    {
      "RoutePattern": "^/admin/.*$",
      "Method": "*", 
      "Rate": 0.5,
      "Description": "Medium sampling for admin operations"
    }
  ]
}
```

## 🔍 Tail-based Sampling

Tail-based sampling makes decisions after spans complete, allowing sampling based on outcomes like status codes, exceptions, and dependency failures.

### Configuration

```json
{
  "OpenTelemetry": {
    "Sampling": {
      "TailSampling": {
        "MaxSpanCount": 10000,
        "DecisionWaitTimeMs": 5000,
        "StatusCodeRules": [
          {
            "StatusCodeRanges": ["400-499"],
            "Rate": 0.8,
            "RoutePattern": "^/api/.*$"
          },
          {
            "StatusCodeRanges": ["500-599"],
            "Rate": 1.0
          }
        ],
        "ExceptionRules": [
          {
            "ExceptionType": "System.ArgumentException", 
            "Rate": 0.5
          },
          {
            "ExceptionType": "System.InvalidOperationException",
            "Rate": 1.0
          },
          {
            "ExceptionType": "*",
            "Rate": 0.9
          }
        ],
        "DependencyRules": [
          {
            "DependencyName": "SQL",
            "FailureRate": 1.0,
            "SuccessRate": 0.1
          }
        ]
      }
    }
  }
}
```

## 🏪 Real-world Examples

### E-commerce Platform

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
          "RoutePattern": "^/api/products.*$",
          "Method": "GET", 
          "Rate": 0.02
        },
        {
          "RoutePattern": "^/api/search.*$",
          "Method": "GET",
          "Rate": 0.1
        },
        {
          "RoutePattern": "^/api/cart.*$",
          "Method": "*",
          "Rate": 0.5
        },
        {
          "RoutePattern": "^/api/checkout.*$",
          "Method": "*",
          "Rate": 1.0
        },
        {
          "RoutePattern": "^/api/payments.*$", 
          "Method": "*",
          "Rate": 1.0
        },
        {
          "RoutePattern": "^/api/orders.*$",
          "Method": "*", 
          "Rate": 1.0
        }
      ],
      "TailSampling": {
        "MaxSpanCount": 15000,
        "DecisionWaitTimeMs": 3000,
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

### High-traffic API Service

```json
{
  "OpenTelemetry": {
    "Sampling": {
      "DefaultRate": 0.01,
      "RespectSamplingHeader": true,
      "RouteSamplingRules": [
        {
          "RoutePattern": "^/v1/users/\\d+$",
          "Method": "GET",
          "Rate": 0.005
        },
        {
          "RoutePattern": "^/v1/analytics.*$",
          "Method": "POST",
          "Rate": 0.1
        },
        {
          "RoutePattern": "^/v1/critical.*$",
          "Method": "*", 
          "Rate": 1.0
        }
      ],
      "TailSampling": {
        "MaxSpanCount": 50000,
        "DecisionWaitTimeMs": 2000,
        "StatusCodeRules": [
          {
            "StatusCodeRanges": ["429"],
            "Rate": 0.1
          },
          {
            "StatusCodeRanges": ["500-599"],
            "Rate": 1.0
          }
        ]
      }
    }
  }
}
```

## 🏗️ Architecture Flow

```
HTTP Request → Route Pattern Match → Head Sampling Decision
                                          ↓
                                   Trace Started/Dropped
                                          ↓  
                                   Request Processing
                                          ↓
                                   Response/Exception
                                          ↓
                               Tail Sampling Evaluation
                                          ↓
                                Final Sampling Decision → Azure Monitor
```

## ⚙️ Configuration Reference

### Head Sampling Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultRate` | `double` | `1.0` | Default sampling rate for unmatched routes (0.0-1.0) |
| `RespectSamplingHeader` | `bool` | `true` | Whether to respect parent trace sampling decisions |
| `RouteSamplingRules` | `array` | `[]` | Array of route-specific sampling rules |

### Route Sampling Rule Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `RoutePattern` | `string` | ✅ | Regex pattern to match request paths |
| `Method` | `string` | ✅ | HTTP method (`GET`, `POST`, `*` for all) |
| `Rate` | `double` | ✅ | Sampling rate for this rule (0.0-1.0) |

### Tail Sampling Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MaxSpanCount` | `int` | `10000` | Maximum pending spans in memory |
| `DecisionWaitTimeMs` | `int` | `5000` | Time to wait for span completion |
| `StatusCodeRules` | `array` | `[]` | Status code-based sampling rules |
| `ExceptionRules` | `array` | `[]` | Exception-based sampling rules |
| `DependencyRules` | `array` | `[]` | Dependency failure sampling rules |

## 🚨 Troubleshooting

### Common Issues

**Issue**: Routes not matching expected patterns
```bash
# Enable logging to see pattern matching
builder.Logging.AddFilter("Asos.OpenTelemetry", LogLevel.Debug);
```

**Issue**: High memory usage with tail sampling
```json
{
  "TailSampling": {
    "MaxSpanCount": 5000,      // Reduce if memory constrained
    "DecisionWaitTimeMs": 2000  // Reduce wait time
  }
}
```

**Issue**: Parent trace sampling conflicts
```json
{
  "Sampling": {
    "RespectSamplingHeader": false  // Override parent decisions
  }
}
```

### Performance Considerations

- **Regex Compilation**: Patterns are compiled once at startup for optimal performance
- **Memory Management**: Tail sampling uses bounded memory with automatic cleanup  
- **Decision Latency**: Tail sampling adds ~5ms decision latency by default
- **CPU Impact**: Head sampling has minimal CPU overhead (~0.1ms per request)

### Best Practices

1. **Start Conservative**: Begin with low default rates and increase specific routes
2. **Monitor Memory**: Watch tail sampling memory usage in production
3. **Test Patterns**: Validate regex patterns with your actual route structure
4. **Gradual Rollout**: Deploy sampling changes gradually to production
5. **Error Sampling**: Always sample errors (rate: 1.0) for debugging capability

## 📊 Monitoring & Metrics

The library exposes metrics for monitoring sampling decisions:

```csharp
// Custom metrics collection
builder.Services.AddOpenTelemetryMetrics(metrics => metrics
    .AddMeter("Asos.OpenTelemetry.AspNetCore")
    .AddAspNetCoreInstrumentation());
```

Available metrics:
- `sampling.head.decisions.total` - Head sampling decisions by route
- `sampling.tail.pending.spans` - Current pending spans count  
- `sampling.tail.decisions.total` - Tail sampling decisions by reason

## 🔧 Advanced Usage

### Custom Sampling Rules

```csharp
public class CustomSamplingRule : IRouteSamplingRule
{
    public bool Matches(string path, string method) 
    {
        // Custom matching logic
        return path.Contains("special") && method == "POST";
    }
    
    public double GetSamplingRate() => 0.75;
}
```

### Integration with Custom Exporters

```csharp
builder.Services.ConfigureOpenTelemetryTracerProvider(builder => builder
    .AddCustomSamplingAzureMonitorTraceExporter()
    .AddOtlpExporter()  // Additional exporters work seamlessly
    .AddConsoleExporter());
```

---

## 🤝 Support

For issues, questions, or contributions, please visit our [GitHub repository](https://github.com/ASOS/asos-open-telemetry).

Built with ❤️ by ASOS Engineering

