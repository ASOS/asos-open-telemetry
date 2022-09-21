using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

[ExcludeFromCodeCoverage]
internal class JwtTokenAcquisition : IAuthenticationTokenAcquisition
{
    private const string EventHubsResource = "https://eventhubs.azure.net/.default";
    private readonly DefaultAzureCredential _defaultAzureCredential = new();

    public AccessToken GetToken(EventHubOptions authenticationMode)
    {
        return _defaultAzureCredential.GetToken(new TokenRequestContext(new[]
            {EventHubsResource}));
    }

    public async Task<AccessToken> GetTokenAsync(EventHubOptions authenticationMode)
    {
        return await _defaultAzureCredential.GetTokenAsync(new TokenRequestContext(new[]
            {EventHubsResource}));
    }
}