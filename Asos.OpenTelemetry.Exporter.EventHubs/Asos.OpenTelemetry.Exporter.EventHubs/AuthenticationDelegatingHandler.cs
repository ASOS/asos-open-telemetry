using System.Net.Http.Headers;
using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

namespace Asos.OpenTelemetry.Exporter.EventHubs;

internal class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly EventHubOptions _eventHubOptions;
    private readonly TokenCache _tokenCache;

    public AuthenticationDelegatingHandler(
        HttpMessageHandler innerHandler,
        TokenCache tokenCache,
        EventHubOptions eventHubOptions)
        : base(innerHandler)
    {
        _tokenCache = tokenCache;
        _eventHubOptions = eventHubOptions;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _tokenCache.GetToken();
        SetRequestHeader(request, token);

        return base.Send(request, cancellationToken);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenCache.GetTokenAsync();
        SetRequestHeader(request, token);

        return await base.SendAsync(request, cancellationToken);
    }

    private void SetRequestHeader(HttpRequestMessage request, string token)
    {
        if (_eventHubOptions.AuthenticationMode == AuthenticationMode.SasKey)
        {
            request.Headers.Add("Authorization", token);
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}