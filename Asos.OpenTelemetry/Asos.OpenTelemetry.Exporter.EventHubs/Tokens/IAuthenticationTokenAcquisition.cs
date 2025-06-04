using Azure.Core;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

/// <summary>
/// Defines the contract for acquiring an authentication token to use when sending data to Event Hub
/// </summary>
public interface IAuthenticationTokenAcquisition
{
    /// <summary>
    /// Gets an authentication token to use when sending data to Event Hub. Type of AccessToken generated
    /// will depend on the authentication mode specified in the EventHubOptions 
    /// </summary>
    /// <param name="authenticationMode">An <see cref="EventHubOptions"/> instance that defines the authentication mode</param>
    /// <returns>An <see cref="AccessToken"/> instance</returns>
    AccessToken GetToken(EventHubOptions authenticationMode);

    /// <summary>
    /// Asynchronously gets an authentication token to use when sending data to Event Hub. Type of AccessToken generated
    /// will depend on the authentication mode specified in the EventHubOptions 
    /// </summary>
    /// <param name="authenticationMode">An <see cref="EventHubOptions"/> instance that defines the authentication mode</param>
    /// <returns>An <see cref="AccessToken"/> instance</returns>
    Task<AccessToken> GetTokenAsync(EventHubOptions authenticationMode);
}