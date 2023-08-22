using System;
using System.Threading.Tasks;
using Asos.OpenTelemetry.Exporter.EventHubs.Tokens;
using NUnit.Framework;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

public class SasTokenAcquisitionTests
{
    [Test]
    public void Generates_Expected_Expiry_Of_One_Hour()
    {
        var testDateTime = new DateTime(2021, 06, 22, 12, 30, 00);
        SystemTime.SetDateTime(testDateTime);

        var tokenAcquisition = new SasTokenAcquisition();

        var accessToken = tokenAcquisition.GetToken(new EventHubOptions
            {AccessKey = "test", KeyName = "Sender", EventHubFqdn = "https://my-resource"});

        var expectedExpiry = new DateTime(2021, 06, 22, 13, 30, 00);
        Assert.AreEqual(expectedExpiry, accessToken.ExpiresOn.DateTime);
    }

    [Test]
    public void Generates_Expected_Token_Format()
    {
        var testDateTime = new DateTime(2021, 06, 22, 12, 30, 00);
        SystemTime.SetDateTime(testDateTime);

        var tokenAcquisition = new SasTokenAcquisition();

        var accessToken = tokenAcquisition.GetToken(new EventHubOptions
            {AccessKey = "test", KeyName = "Sender", EventHubFqdn = "https://my-resource", AuthenticationMode = AuthenticationMode.SasKey});

        var expected =
            "sr=https%3A%2F%2Fmy-resource&sig=9GN48obx4qmr8AnCbslsNx8nij25uqayZnK7Aur%2FjjQ%3D&se=1624368600&skn=Sender";
        Assert.AreEqual(expected, accessToken.Token);
    }
    
    [Test]
    public async Task Generates_Expected_Token_Format_Async()
    {
        var testDateTime = new DateTime(2021, 06, 22, 12, 30, 00);
        SystemTime.SetDateTime(testDateTime);

        var tokenAcquisition = new SasTokenAcquisition();

        var accessToken = await tokenAcquisition.GetTokenAsync(new EventHubOptions
            {AccessKey = "test", KeyName = "Sender", EventHubFqdn = "https://my-resource"});

        var expected =
            "sr=https%3A%2F%2Fmy-resource&sig=9GN48obx4qmr8AnCbslsNx8nij25uqayZnK7Aur%2FjjQ%3D&se=1624368600&skn=Sender";
        Assert.AreEqual(expected, accessToken.Token);
    }
}