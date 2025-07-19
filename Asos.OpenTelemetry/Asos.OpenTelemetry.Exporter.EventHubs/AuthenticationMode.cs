namespace Asos.OpenTelemetry.Exporter.EventHubs;

/// <summary>
/// Authentication modes supported when exporting data to Event Hub
/// </summary>
public enum AuthenticationMode
{
    /// <summary>
    /// SasKey format. Generate a shared access signature, using a key name and access key
    /// </summary>
    SasKey = 0,
    /// <summary>
    /// Managed Identity. Will used DefaultAzureCredential to acquire an access token scoped for
    /// event hubs resources and send as Bearer token
    /// </summary>
    ManagedIdentity = 1
}