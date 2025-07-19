using Azure.Core;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal class SasTokenAcquisition : IAuthenticationTokenAcquisition
{
    private readonly SasKeyGenerator _sasKeyGenerator = new();

    public AccessToken GetToken(EventHubOptions options)
    {
        return GenerateToken(options);
    }

    public Task<AccessToken> GetTokenAsync(EventHubOptions options)
    {
        var token = GenerateToken(options);

        return Task.FromResult(token);
    }

    private AccessToken GenerateToken(EventHubOptions options)
    {
        return _sasKeyGenerator.CreateSasToken(
            options.EventHubFqdn,
            options.KeyName,
            options.AccessKey);
    }
}