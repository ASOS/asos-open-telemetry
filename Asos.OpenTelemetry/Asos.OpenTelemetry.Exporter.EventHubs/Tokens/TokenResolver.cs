namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal static class TokenResolver
{
    public static IAuthenticationTokenAcquisition ResolveTokenProvider(EventHubOptions options)
    {
        return options.AuthenticationMode switch
        {
            AuthenticationMode.SasKey => new SasTokenAcquisition(),
            AuthenticationMode.ManagedIdentity => new JwtTokenAcquisition(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}