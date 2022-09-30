using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace Asos.OpenTelemetry.Exporter.EventHubs.Tokens;

internal class SasKeyGenerator
{
    private const int TokenLifetime = 3600;

    public AccessToken CreateSasToken(string resourceUri, string keyName, string key)
    {
        var tokenGenerationTime = SystemTime.UtcNow();
        
        var sinceEpoch = tokenGenerationTime - new DateTime(1970, 1, 1);
        var expiry = Convert.ToString((int) sinceEpoch.TotalSeconds + TokenLifetime);
        var stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;
        var signature = GetSignature(key, stringToSign);

        var token = string.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
            WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry, keyName);

        var offset = new DateTimeOffset(tokenGenerationTime);
        return new AccessToken(token, offset.AddSeconds(TokenLifetime));
    }

    private static string GetSignature(string key, string stringToSign)
    {
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));

        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
    }
}