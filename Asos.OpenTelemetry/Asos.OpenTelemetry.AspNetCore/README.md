# Open Telemetry extensions for Asp Net Core

A library for configuring OpenTelemetry in ASP.NET Core applications,

## What's it for?

This library is intended to help modify the default behaviour of OpenTelemetry in ASP.NET Core applications, allowing
some customisation of the way data is exported, sampled and other behaviours.

## How does it work?

Extension methods are available that allow you to change the behaviour of OpenTelemetry via the WebApplicationBuilder

```csharp
builder.ConfigureOpenTelemetryCustomSampling(
    options =>
    {
        // whatever options you want to set
    });
```

The `ConfigureOpenTelemetryCustomSampling` method allows you to set up custom sampling rules, which can be used to control
the sampling rate of different routes or HTTP methods in your application.

To define the rules, create a section in your `appsettings.json` file under the `OpenTelemetry:Sampling` path.

```json
{
  "OpenTelemetry": {
    "Sampling": {
      "DefaultRate": 0.05,
      "SamplingRules": [
        {
          "RoutePattern": "^/api/customers/\\d+$",
          "Method": "GET",
          "Rate": 1.0
        },
        {
          "RoutePattern": "^/api/orders$",
          "Method": "POST",
          "Rate": 0.25
        },
        {
          "RoutePattern": "^/health$",
          "Method": "GET",
          "Rate": 0.0
        }
      ]
    }
  }
}
```

By doing so, you can control the sampling rate for specific routes and HTTP methods in your ASP.NET Core application.

Be aware that different sampling rates can break the consistency of your traces, so use this feature with caution. It's a good
option when you don't call into external APIs and just call your own dependencies, as it can help reduce the amount of data
you produce

For example, if you have a GET endpoint that only calls a database and no other services, is successful a very high percentage of 
time and you don't need to see every single request, you can set the sampling rate to 0.05 (5%) for that endpoint.

You might have another endpoint that performs a POST operation and calls into an external API, which is less reliable and you want to see
every request, so you can set the sampling rate to 1.0 (100%) for that endpoint.





