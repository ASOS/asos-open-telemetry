using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

public class DummyHandler : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return CopyRequestHeader(request);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(CopyRequestHeader(request));
    }

    private HttpResponseMessage CopyRequestHeader(HttpRequestMessage request)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted);

        foreach (var httpRequestHeader in request.Headers)
        {
            response.Headers.Add(httpRequestHeader.Key, httpRequestHeader.Value);
        }

        return response;
    }
}