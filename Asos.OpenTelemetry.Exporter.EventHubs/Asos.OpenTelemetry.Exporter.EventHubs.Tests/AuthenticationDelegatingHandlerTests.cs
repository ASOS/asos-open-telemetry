using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using Azure.Core;
using Moq;
using NUnit.Framework;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

public class AuthenticationDelegatingHandlerTests
{
    [Test]
    public void Send_Sets_Sas_Authentication_Header()
    {
        var opts = new EventHubOptions(){AuthenticationMode = AuthenticationMode.SasKey};
        
        var tokenMock = new Mock<IAuthenticationTokenAcquisition>();
        tokenMock.Setup(s => s.GetToken(opts))
            .Returns(new AccessToken("test-token", DateTimeOffset.Now.AddMinutes(5)));
        
        var tokenCache = new TokenCache(opts, tokenMock.Object);
        
        var httpClient =
            new HttpClient(new AuthenticationDelegatingHandler(new DummyHandler(), tokenCache, opts));

        var authHeader = MakeRequestAndGetAuthHeaders(httpClient);
        
        Assert.AreEqual("test-token", authHeader[0]);
    }
    
    [Test]
    public void Send_Sets_Managed_Identity_Authentication_Header()
    {
        var opts = new EventHubOptions(){AuthenticationMode = AuthenticationMode.ManagedIdentity};
        
        var tokenMock = new Mock<IAuthenticationTokenAcquisition>();
        tokenMock.Setup(s => s.GetToken(opts))
            .Returns(new AccessToken("test-token", DateTimeOffset.Now.AddMinutes(5)));
        
        var tokenCache = new TokenCache(opts, tokenMock.Object);
        
        var httpClient =
            new HttpClient(new AuthenticationDelegatingHandler(new DummyHandler(), tokenCache, opts));
        
        var authHeader = MakeRequestAndGetAuthHeaders(httpClient);
        
        Assert.AreEqual("Bearer test-token", authHeader[0]);
    }

    [Test]
    public async Task SendAsync_Sets_Managed_Identity_Authentication_Header()
    {
        var opts = new EventHubOptions(){AuthenticationMode = AuthenticationMode.ManagedIdentity};
        
        var tokenMock = new Mock<IAuthenticationTokenAcquisition>();
        tokenMock.Setup(s => s.GetTokenAsync(opts))
            .ReturnsAsync(new AccessToken("test-token", DateTimeOffset.Now.AddMinutes(5)));
        
        var tokenCache = new TokenCache(opts, tokenMock.Object);
        
        var httpClient =
            new HttpClient(new AuthenticationDelegatingHandler(new DummyHandler(), tokenCache, opts));
        
        var authHeader = await MakeAsyncRequestAndGetAuthHeaders(httpClient);
        
        Assert.AreEqual("Bearer test-token", authHeader[0]);
    }
    
    private List<string> MakeRequestAndGetAuthHeaders(HttpClient httpClient)
    {
        var message = new HttpRequestMessage();
        message.RequestUri = new Uri("https://local-host/dummy/endpoint");
        var result = httpClient.Send(message);
        return result.Headers.GetValues("Authorization").ToList();
    }
    
    private async Task<List<string>> MakeAsyncRequestAndGetAuthHeaders(HttpClient httpClient)
    {
        var message = new HttpRequestMessage();
        message.RequestUri = new Uri("https://local-host/dummy/endpoint");
        var result = await httpClient.SendAsync(message);
        return result.Headers.GetValues("Authorization").ToList();
    }
}