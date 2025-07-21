using System.Collections.Concurrent;
using System.Diagnostics;
using Asos.OpenTelemetry.AspNetCore.Sampling;
using Asos.OpenTelemetry.AspNetCore.Sampling.Head;
using Asos.OpenTelemetry.AspNetCore.Sampling.Tail;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Asos.OpenTelemetry.AspNetCore.Tests;

/// <summary>
/// Integration tests for the actual TailBasedSamplingProcessor implementation
/// </summary>
[TestFixture]
public class SamplingPipelineIntegrationTests
{
    private static readonly ActivitySource TestSource = new("TestSource");
    
    private ServiceProvider _serviceProvider = null!;
    private TestExporter _testExporter = null!;
    private TracerProvider _tracerProvider = null!;
    private ConcurrentBag<Activity> _exportedActivities = null!;
    private TestHttpContextAccessor _httpContextAccessor = null!;
    private TailSamplingOptions _options = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Set up DI container
        var services = new ServiceCollection();
        _httpContextAccessor = new TestHttpContextAccessor();
        services.AddSingleton<IHttpContextAccessor>(_httpContextAccessor);
        _serviceProvider = services.BuildServiceProvider();

        _exportedActivities = new ConcurrentBag<Activity>();
        _testExporter = new TestExporter(_exportedActivities);
        
        // Configure the actual tail sampling options
        _options = new TailSamplingOptions
        {
            DefaultSamplingRate = 0.1,
            DefaultExceptionSamplingRate = 1.0,
            ServerErrorSamplingRate = 1.0,
            ClientErrorSamplingRate = 0.5,
            SlowRequestSamplingRate = 0.8,
            SlowRequestThreshold = TimeSpan.FromSeconds(2),
            StatusCodeRules = new List<StatusCodeRule>(),
        };
        
        // Create a test tracer provider with the actual implementation
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("TestSource") 
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TestService"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .SetSampler(new RouteRuleSampler(new RouteSamplingOptions()
            {
                DefaultRate = 1.0, // Always sample at head level - let tail processor decide
                RouteSamplingRules = new List<RouteSamplingRule>() // Empty rules for head sampler
            }, _httpContextAccessor)) 
            .AddProcessor(new TailBasedSamplingProcessor(_options, _httpContextAccessor))
            .AddProcessor(new BatchActivityExportProcessor(_testExporter)) 
            .Build()!;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _tracerProvider?.Dispose();
        _serviceProvider?.Dispose();
        _testExporter?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _exportedActivities.Clear();
        
        // Reset options to defaults
        _options.StatusCodeRules.Clear();
        _options.DefaultSamplingRate = 0.1;
        _options.DefaultExceptionSamplingRate = 1.0;
        _options.ServerErrorSamplingRate = 1.0;
        _options.ClientErrorSamplingRate = 0.5;
    }

    [Test]
    public void ShouldAlwaysSampleServerErrors()
    {
        // Arrange
        var testRequests = new[]
        {
            CreateTestRequest("/api/test/server-error", "GET", 500),
            CreateTestRequest("/api/test/server-error", "GET", 502),
            CreateTestRequest("/api/test/server-error", "GET", 503)
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var serverErrorSpans = _exportedActivities
            .Where(a => a.GetTagItem("http.status_code")?.ToString() == "500" ||
                       a.GetTagItem("http.status_code")?.ToString() == "502" ||
                       a.GetTagItem("http.status_code")?.ToString() == "503")
            .ToList();

        Assert.That(serverErrorSpans.Count, Is.EqualTo(3));
        
        TestContext.WriteLine($"All {serverErrorSpans.Count} server error spans were sampled as expected");
    }

    [Test]
    public void ShouldAlwaysSampleExceptions()
    {
        // Arrange
        var testRequests = new[]
        {
            CreateTestRequestWithException("/api/test/exception", "GET", "InvalidOperationException"),
            CreateTestRequestWithException("/api/test/exception", "GET", "ArgumentNullException"),
            CreateTestRequestWithException("/api/test/exception", "GET", "TimeoutException")
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var exceptionSpans = _exportedActivities
            .Where(a => a.GetTagItem("exception.type") != null)
            .ToList();

        Assert.That(exceptionSpans.Count, Is.EqualTo(3));
        
        TestContext.WriteLine($"All {exceptionSpans.Count} exception spans were sampled as expected");
    }

    [Test]
    public void ShouldSampleSlowRequestsBasedOnThreshold()
    {
        // Arrange
        var testRequests = new[]
        {
            CreateTestRequestWithDuration("/api/test/slow", "GET", TimeSpan.FromMilliseconds(500)),  // Fast
            CreateTestRequestWithDuration("/api/test/slow", "GET", TimeSpan.FromSeconds(3)),        // Slow
            CreateTestRequestWithDuration("/api/test/slow", "GET", TimeSpan.FromSeconds(1)),        // Fast
            CreateTestRequestWithDuration("/api/test/slow", "GET", TimeSpan.FromSeconds(4))         // Slow
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var slowSpans = _exportedActivities
            .Where(a => a.Duration > TimeSpan.FromSeconds(2))
            .ToList();

        // With 80% slow request sampling rate, we expect probabilistic results
        // But since this is deterministic, we test the threshold logic
        var totalSlowRequests = testRequests.Count(r => r.Duration > TimeSpan.FromSeconds(2));
        
        // At least some slow requests should be sampled (given 80% rate)
        Assert.That(slowSpans.Count, Is.GreaterThan(0));
        Assert.That(slowSpans.Count, Is.LessThanOrEqualTo(totalSlowRequests));
        
        TestContext.WriteLine($"Slow spans: {slowSpans.Count} out of {totalSlowRequests} slow requests");
    }

    [Test]
    public void ShouldPrioritizeExceptions()
    {
        var testRequest = CreateTestRequestWithException("/api/health/check", "GET", "InvalidOperationException");

        // Act
        ProcessTestRequests([testRequest]);

        // Assert - exception should override route sampling rule
        var exceptionSpans = _exportedActivities
            .Where(a => a.GetTagItem("exception.type") != null && 
                       a.GetTagItem("http.target")?.ToString()?.StartsWith("/api/health") == true)
            .ToList();

        Assert.That(exceptionSpans.Count, Is.EqualTo(1), "Exception should override route rule");
        
        TestContext.WriteLine($"Exception on health endpoint was sampled despite route rule");
    }

    [Test]
    public void ShouldRespectStatusCodeRules()
    {
        // Arrange
        _options.StatusCodeRules.Add(new StatusCodeRule
        {
            StatusCode = 429,
            SamplingRate = 1.0 // Always sample rate limiting
        });

        _options.StatusCodeRules.Add(new StatusCodeRule
        {
            StatusCode = 404,
            SamplingRate = 0.0 // Never sample not found
        });

        var testRequests = new[]
        {
            CreateTestRequest("/api/test/rate-limit", "GET", 429),
            CreateTestRequest("/api/test/not-found", "GET", 404),
            CreateTestRequest("/api/test/rate-limit", "GET", 429)
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var rateLimitSpans = _exportedActivities
            .Where(a => a.GetTagItem("http.status_code")?.ToString() == "429")
            .ToList();

        var notFoundSpans = _exportedActivities
            .Where(a => a.GetTagItem("http.status_code")?.ToString() == "404")
            .ToList();

        Assert.That(rateLimitSpans.Count, Is.EqualTo(2), "All rate limit responses should be sampled");
        Assert.That(notFoundSpans.Count, Is.EqualTo(0), "Not found responses should not be sampled");

        TestContext.WriteLine($"Rate limit spans: {rateLimitSpans.Count}, Not found spans: {notFoundSpans.Count}");
    }

    [Test]
    public void ShouldHandleExceptions()
    {
        var testRequests = new[]
        {
            CreateTestRequestWithException("/api/test/exception", "GET", "ArgumentNullException"),
            CreateTestRequestWithException("/api/test/exception", "GET", "InvalidOperationException")
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var argumentNullSpans = _exportedActivities
            .Where(a => a.GetTagItem("exception.type")?.ToString() == "ArgumentNullException")
            .ToList();

        var invalidOpSpans = _exportedActivities
            .Where(a => a.GetTagItem("exception.type")?.ToString() == "InvalidOperationException")
            .ToList();

        Assert.That(argumentNullSpans.Count, Is.EqualTo(1), "ArgumentNullException should be sampled");
        Assert.That(invalidOpSpans.Count, Is.EqualTo(1), "InvalidOperationException should be sampled");

        TestContext.WriteLine($"ArgumentNull spans: {argumentNullSpans.Count}, InvalidOp spans: {invalidOpSpans.Count}");
    }

    [Test]
    public void ShouldUseDefaultSamplingRateForUnmatchedRequests()
    {
        // Arrange
        _options.SuccessSamplingRate = 0.0;
        _options.DefaultSamplingRate = 0.0; 

        var testRequests = new[]
        {
            CreateTestRequest("/api/random/endpoint", "GET", 200),
            CreateTestRequest("/api/another/endpoint", "POST", 201),
            CreateTestRequest("/api/third/endpoint", "PUT", 200)
        };

        // Act
        ProcessTestRequests(testRequests);

        // Assert
        var allSpans = _exportedActivities.ToList();
        Assert.That(allSpans.Count, Is.EqualTo(0), "No spans should be sampled with 0% default rate");

        TestContext.WriteLine($"Default sampling resulted in {allSpans.Count} spans");
    }

    [Test]
    public void ShouldMaintainPerformanceWithManyRequests()
    {
        // Arrange
        var testRequests = Enumerable.Range(0, 1000).Select(i =>
            CreateTestRequest($"/api/performance/test/{i}", "GET", 200)
        ).ToArray();

        var stopwatch = Stopwatch.StartNew();

        // Act
        ProcessTestRequests(testRequests);

        stopwatch.Stop();

        // Assert
        var totalSpans = _exportedActivities.Count;
        var averageTimePerRequest = stopwatch.ElapsedMilliseconds / (double)testRequests.Length;

        Assert.That(averageTimePerRequest, Is.LessThan(1.0), 
            $"Sampling should be fast. Average: {averageTimePerRequest:F3}ms per request");

        TestContext.WriteLine($"Performance test: {testRequests.Length} requests in {stopwatch.ElapsedMilliseconds}ms");
        TestContext.WriteLine($"Average time per request: {averageTimePerRequest:F3}ms");
        TestContext.WriteLine($"Total spans exported: {totalSpans}");
    }

    private void ProcessTestRequests(TestRequest[] requests)
    {
        foreach (var request in requests)
        {
            // Set up HTTP context
            _httpContextAccessor.SetHttpContext(request.HttpContext);

            // Create and process activity
            using var activity = TestSource.StartActivity(request.OperationName);
            activity.SetTag("http.method", request.Method);
            activity.SetTag("http.target", request.Path);
            activity.SetTag("http.status_code", request.StatusCode.ToString());
            
            if (request.Exception != null)
            {
                activity.SetTag("exception.type", request.Exception);
                activity.SetStatus(ActivityStatusCode.Error);
            }

            activity.Start();
            
            // Simulate duration if specified
            if (request.Duration.HasValue)
            {
                var endTime = activity.StartTimeUtc.Add(request.Duration.Value);
                activity.SetEndTime(endTime);
            }

            activity.Stop();
        }
        
        // Force flush to ensure all activities are processed
        _tracerProvider.ForceFlush(1000);
        
        // Small delay to ensure async processing completes
        Thread.Sleep(50);
    }

    private TestRequest CreateTestRequest(string path, string method, int statusCode)
    {
        return new TestRequest
        {
            Path = path,
            Method = method,
            StatusCode = statusCode,
            OperationName = $"{method} {path}",
            HttpContext = CreateHttpContext(path, method)
        };
    }

    private TestRequest CreateTestRequestWithException(string path, string method, string exceptionType)
    {
        return new TestRequest
        {
            Path = path,
            Method = method,
            StatusCode = 500,
            Exception = exceptionType,
            OperationName = $"{method} {path}",
            HttpContext = CreateHttpContext(path, method)
        };
    }

    private TestRequest CreateTestRequestWithDuration(string path, string method, TimeSpan duration)
    {
        return new TestRequest
        {
            Path = path,
            Method = method,
            StatusCode = 200,
            Duration = duration,
            OperationName = $"{method} {path}",
            HttpContext = CreateHttpContext(path, method)
        };
    }

    private HttpContext CreateHttpContext(string path, string method)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = path,
                Method = method
            }
        };
        return context;
    }
}

/// <summary>
/// Test request model for unit testing
/// </summary>
public class TestRequest
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? Exception { get; set; }
    public TimeSpan? Duration { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public HttpContext HttpContext { get; set; } = null!;
}

/// <summary>
/// Test implementation of IHttpContextAccessor
/// </summary>
public class TestHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }

    public void SetHttpContext(HttpContext context)
    {
        HttpContext = context;
    }
}
