namespace Asos.OpenTelemetry.Exporter.EventHubs;

/// <summary>
///     Options for setting Event Hub target and authentication mode when transmitting OTLP data
/// </summary>
public class EventHubOptions
{
    /// <summary>
    /// The authentication mode to use when sending data to Event Hub. Can be either Share Access Signature,
    /// or Managed Identity
    /// </summary>
    public AuthenticationMode AuthenticationMode { get; set; } = AuthenticationMode.SasKey;

    /// <summary>
    ///     Only required If <see cref="AuthenticationMode" /> is SasKey - the name of the Access Key.
    /// </summary>
    public string KeyName { get; set; } = "TelemetrySender";

    /// <summary>
    ///     Only required If <see cref="AuthenticationMode" /> is SasKey - the access key value.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    ///     The fully qualified Uri of the event hub to send OLTP data to.  An event hub named MyEventHub on host
    ///     event-hub-host-name would give a EventHubFqdn value of
    ///     https://event-hub-host-name.servicebus.windows.net/MyEventHub
    /// </summary>
    public string EventHubFqdn { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the metric export interval in milliseconds. Defaults to 10000
    /// </summary>
    public int ExportIntervalMilliseconds { get; set; } = 10000;
    
    internal void Validate()
    {
        switch (AuthenticationMode)
        {
            case AuthenticationMode.SasKey:
                ValidateSasKeyMode();
                break;
            case AuthenticationMode.ManagedIdentity:
                ValidateManagedIdentityMode();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(AuthenticationMode),
                    "Unknown Authentication mode specified");
        }
    }

    private void ValidateSasKeyMode()
    {
        if (string.IsNullOrEmpty(AccessKey) || string.IsNullOrEmpty(EventHubFqdn) ||
            string.IsNullOrEmpty(KeyName))
        {
            throw new InvalidOperationException(
                "When authentication mode is SasKey, you must provide values for KeyName, AccessKey and EventHubFqdn");
        }
        
        ValidateEventHubTarget();
    }
    
    private void ValidateManagedIdentityMode()
    {
        if (string.IsNullOrEmpty(EventHubFqdn))
        {
            throw new InvalidOperationException(
                "When authentication mode is ManagedIdentity, you must provide a value for EventHubFqdn");
        }

        ValidateEventHubTarget();
    }
    
    private void ValidateEventHubTarget()
    {
        if (!Uri.IsWellFormedUriString(EventHubFqdn, UriKind.Absolute))
        {
            throw new InvalidOperationException(
                "You must provide a well formed URI for EventHubFqdn");  
        }
    }
}