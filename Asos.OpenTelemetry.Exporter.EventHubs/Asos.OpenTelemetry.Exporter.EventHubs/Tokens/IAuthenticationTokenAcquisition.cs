using Azure.Core;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

public interface IAuthenticationTokenAcquisition
{
    AccessToken GetToken(EventHubOptions authenticationMode);

    Task<AccessToken> GetTokenAsync(EventHubOptions authenticationMode);
}