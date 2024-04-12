using System;
using System.Threading.Tasks;
using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using Azure.Core;
using Moq;
using NUnit.Framework;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

[TestFixture]
public class TokenCacheTests
{
    private readonly Mock<IAuthenticationTokenAcquisition> _mockTokenGenerator = new();
    private TokenCache _tokenCache;
    private const string NewToken = "newtoken";
    private readonly DateTime _testDateTime = new(2021, 06, 22, 12, 30, 00);
    
    public TokenCacheTests()
    {
        _tokenCache = new TokenCache(new EventHubOptions(), _mockTokenGenerator.Object); 
    }
    
    [Test]
    public async Task Return_NewToken_When_CachedToken_IsNull()
    {
        SystemTime.SetDateTime(_testDateTime);
        var tokenResult = new AccessToken(NewToken, _testDateTime);
        
        _mockTokenGenerator.Setup(gen => 
            gen.GetTokenAsync(It.IsAny<EventHubOptions>()))
            .ReturnsAsync(tokenResult);

        var result = await _tokenCache.GetTokenAsync();

        Assert.That("newtoken", Is.EqualTo(result));
    }

    [TestCase(-10)]
    [TestCase(0)]
    public async Task ReturnAndCacheNewTokenWhenCachedTokenIsExpired(int tokenExpiry)
    {
        SystemTime.SetDateTime(_testDateTime);
        var tokenResult = new AccessToken(NewToken, _testDateTime.AddSeconds(tokenExpiry));
        
        _mockTokenGenerator.Setup(gen => 
                gen.GetTokenAsync(It.IsAny<EventHubOptions>()))
            .ReturnsAsync(tokenResult);

        var result = await _tokenCache.GetTokenAsync();

        Assert.That(NewToken, Is.EqualTo(result));
    }
    
    [Test]
    public async Task ReturnsCachedTokenWhenCachedTokenIsValid()
    {
        const string newToken = "token-from-cache";
        SystemTime.SetDateTime(_testDateTime);
        
        var tokenResult = new AccessToken(newToken, _testDateTime.AddHours(1));
        
        _tokenCache = new TokenCache(new EventHubOptions(), _mockTokenGenerator.Object, tokenResult);
        
        var result = await _tokenCache.GetTokenAsync();

        Assert.That("token-from-cache", Is.EqualTo(result));
    }
}