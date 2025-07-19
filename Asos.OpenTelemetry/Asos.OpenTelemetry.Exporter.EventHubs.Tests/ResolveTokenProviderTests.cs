using System;
using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using NUnit.Framework;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

public class TokenResolverTests
{
    [Test]
    public void Resolves_SasKey_Provider_For_Authentication_Type()
    {
        var provider = TokenResolver.ResolveTokenProvider(new EventHubOptions
            {AuthenticationMode = AuthenticationMode.SasKey});

        Assert.That(provider, Is.InstanceOf<SasTokenAcquisition>());
    }

    [Test]
    public void Resolves_Jwt_Provider_For_Authentication_Type()
    {
        var provider = TokenResolver.ResolveTokenProvider(new EventHubOptions
            {AuthenticationMode = AuthenticationMode.ManagedIdentity});

        Assert.That(provider, Is.InstanceOf<JwtTokenAcquisition>());
    }

    [Test]
    public void Throws_Exception_For_Unknown()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            TokenResolver.ResolveTokenProvider(new EventHubOptions {AuthenticationMode = (AuthenticationMode) 2});
        });
    }
}