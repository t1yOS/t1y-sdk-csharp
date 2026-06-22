using System;
using System.Security.Cryptography;
using System.Text;

namespace T1YOS.Sdk.Crypto
{

/// <summary>
/// HMAC-SHA256 utilities for the T1YOS SDK.
/// Used for request signing and signature verification.
/// </summary>
public static class HMACHelper
{
    /// <summary>
    /// Computes the HMAC-SHA256 of a message using a secret key,
    /// returning the result as a lowercase hexadecimal string (64 characters).
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="message">The message to authenticate.</param>
    /// <returns>Lowercase hexadecimal HMAC-SHA256 hash string.</returns>
    public static string HmacSHA256Hex(string secret, string message)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(secretBytes))
        {
            var hash = hmac.ComputeHash(messageBytes);
            return BytesToHexStringLower(hash);
        }
    }

    /// <summary>
    /// Verifies an HMAC-SHA256 signature against the expected value.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="message">The original message.</param>
    /// <param name="signature">The signature to verify (case-insensitive).</param>
    /// <returns>True if the signature is valid.</returns>
    public static bool VerifyHmacSHA256(string secret, string message, string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            return false;
        }

        var expected = HmacSHA256Hex(secret, message);
        return ConstantTimeEquals(expected, signature.ToLowerInvariant());
    }

    /// <summary>
    /// Converts a byte array to a lowercase hexadecimal string.
    /// </summary>
    private static string BytesToHexStringLower(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Performs a constant-time comparison of two strings.
    /// Mitigates timing side-channel attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }
}

}
