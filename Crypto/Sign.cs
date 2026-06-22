using System;
using System.Text;

namespace T1YOS.Sdk.Crypto
{

/// <summary>
/// Input parameters for request signature generation.
/// </summary>
public class SignatureInput
{
    /// <summary>HTTP method (GET, POST, PUT, DELETE).</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>URL path with query string.</summary>
    public string PathAndQuery { get; set; } = string.Empty;

    /// <summary>Request body string.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Application ID.</summary>
    public int AppId { get; set; }

    /// <summary>Unix timestamp for the request.</summary>
    public long Timestamp { get; set; }

    /// <summary>Secret key for HMAC-SHA256 signing.</summary>
    public string SecretKey { get; set; } = string.Empty;
}

/// <summary>
/// Request signing utilities for the T1YOS SDK.
/// Generates HMAC-SHA256 signatures used to authenticate API requests.
///
/// Signature algorithm:
/// <code>
/// message = METHOD + "\n" + PATH_AND_QUERY + "\n" + SHA256(body) + "\n" + appId + "\n" + timestamp
/// signature = HMAC-SHA256(secretKey, message)
/// </code>
/// </summary>
public static class SignHelper
{
    /// <summary>
    /// Creates an HMAC-SHA256 request signature.
    /// </summary>
    /// <param name="input">The signature input parameters.</param>
    /// <returns>A 64-character lowercase hexadecimal signature string.</returns>
    public static string CreateSignature(SignatureInput input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        // Compute SHA-256 of the body (even if empty)
        var bodyHash = SHA256Helper.Sha256Hex(input.Body ?? string.Empty);

        // Build the signing message
        var message = new StringBuilder()
            .Append(input.Method.ToUpperInvariant())
            .Append('\n')
            .Append(input.PathAndQuery)
            .Append('\n')
            .Append(bodyHash)
            .Append('\n')
            .Append(input.AppId)
            .Append('\n')
            .Append(input.Timestamp)
            .ToString();

        // HMAC-SHA256 with the secret key
        return HMACHelper.HmacSHA256Hex(input.SecretKey, message);
    }

    /// <summary>
    /// Gets a safe Unix timestamp adjusted by the server time offset.
    /// The offset is computed during <c>InitAsync()</c> to correct for
    /// clock skew between client and server.
    /// </summary>
    /// <param name="offset">Time offset in seconds (server time - client time).</param>
    /// <returns>Adjusted Unix timestamp as a string.</returns>
    public static string GetSafeTimestamp(int offset)
    {
        var unixNow = GetUnixTimestampSeconds();
        return (unixNow + offset).ToString();
    }

    /// <summary>
    /// Gets the current Unix timestamp in seconds.
    /// Uses <c>DateTimeOffset.ToUnixTimeSeconds</c> on .NET 5+,
    /// falls back to manual calculation on older frameworks.
    /// </summary>
    private static long GetUnixTimestampSeconds()
    {
#if NET8_0_OR_GREATER
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#else
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(DateTime.UtcNow - epoch).TotalSeconds;
#endif
    }
}

}
