using Azure.Core;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal class TokenCache
{
    private readonly EventHubOptions _option;
    private readonly IAuthenticationTokenAcquisition _tokenAcquirer;
    private AccessToken _cachedToken;

    public TokenCache(
        EventHubOptions option, 
        IAuthenticationTokenAcquisition tokenAcquirer)
    {
        _option = option;
        _tokenAcquirer = tokenAcquirer;
    }
    
    internal TokenCache(
        EventHubOptions option, 
        IAuthenticationTokenAcquisition tokenAcquirer, 
        AccessToken token) : this(option, tokenAcquirer)
    {
        _cachedToken = token;
    }

    public string GetToken()
    {
        if (TokenExists() && !TokenExpired() && !TokenCloseToExpiring())
        {
            return _cachedToken.Token;
        }

        return GetAndCacheAccessToken();
    }

    public async Task<string> GetTokenAsync()
    {
        if (TokenExists() && !TokenExpired() && !TokenCloseToExpiring())
        {
            return _cachedToken.Token;
        }

        return await GetAndCacheAccessTokenAsync();
    }

    private string GetAndCacheAccessToken()
    {
        _cachedToken = _tokenAcquirer.GetToken(_option);

        return _cachedToken.Token;
    }

    private async Task<string> GetAndCacheAccessTokenAsync()
    {
        _cachedToken = await _tokenAcquirer.GetTokenAsync(_option);

        return _cachedToken.Token;
    }

    private bool TokenExists()
    {
        return !string.IsNullOrWhiteSpace(_cachedToken.Token);
    }

    private bool TokenExpired()
    {
        return SystemTime.UtcNow() >= _cachedToken.ExpiresOn;
    }

    private bool TokenCloseToExpiring()
    {
        return SystemTime.UtcNow().AddMinutes(5) >= _cachedToken.ExpiresOn;
    }
}