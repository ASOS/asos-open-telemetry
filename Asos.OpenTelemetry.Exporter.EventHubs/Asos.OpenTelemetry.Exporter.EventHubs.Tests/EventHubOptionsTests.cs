using System;
using NUnit.Framework;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tests;

public class EventHubOptionsTests
{
    private const string WellFormedTargetEndpoint = "https://some-host.servicebus.windows.net/EventHubName";

    [Test]
    public void Should_Throw_Exception_When_Missing_AccessKey_Setting_For_Sas_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.SasKey, 
            KeyName = "KeyName", 
            AccessKey = "", 
            EventHubFqdn = WellFormedTargetEndpoint
        };

        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }
    
    [Test]
    public void Should_Throw_Exception_When_Missing_KeyName_Setting_For_Sas_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.SasKey, 
            KeyName = "", 
            AccessKey = "Key", 
            EventHubFqdn = WellFormedTargetEndpoint
        };

        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }
    
    [Test]
    public void Should_Throw_Exception_When_Missing_Fqdn_Setting_For_Sas_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.SasKey, 
            KeyName = "KeyName", 
            AccessKey = "Key", 
            EventHubFqdn = ""
        };

        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }
    
    [Test]
    public void Should_Throw_Exception_When_Bad_Fqdn_Setting_For_Sas_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.SasKey, 
            KeyName = "KeyName", 
            AccessKey = "Key", 
            EventHubFqdn = "this-is-not-valid"
        };
        
        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }
    
    [Test]
    public void Should_Not_Throw_Exception_When_Settings_Good_For_Sas_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.SasKey, 
            KeyName = "KeyName", 
            AccessKey = "Key", 
            EventHubFqdn = WellFormedTargetEndpoint
        };

        options.Validate();
    }
 
    [Test]
    public void Should_Throw_Exception_When_Missing_Fqdn_Setting_For_Managed_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.ManagedIdentity,
            EventHubFqdn = ""
        };

        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }
    
    [Test]
    public void Should_Throw_Exception_When_Bad_Fqdn_Setting_For_Managed_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.ManagedIdentity,
            EventHubFqdn = "this-is-not-valid"
        };

        TestDelegate act = () => options.Validate();
        Assert.Throws<InvalidOperationException>(act);
    }

    
    [Test]
    public void Should_Not_Throw_Exception_When_Settings_Good_For_Managed_Authentication()
    {
        var options = new EventHubOptions()
        {
            AuthenticationMode = AuthenticationMode.ManagedIdentity,
            EventHubFqdn = WellFormedTargetEndpoint
        };

        options.Validate();
    }
}