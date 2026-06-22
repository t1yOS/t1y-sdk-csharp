using System.Security.Cryptography;
using System.Text;

namespace T1YOS.Sdk.Crypto
{

/// <summary>
/// SHA-256 hashing utilities for the T1YOS SDK.
/// Provides hex-encoded SHA-256 digest computation.
/// </summary>
public static class SHA256Helper
{
    /// <summary>
    /// Computes the SHA-256 hash of a string and returns it as a
    /// lowercase hexadecimal string (64 characters).
    /// </summary>
    /// <param name="data">The input string to hash.</param>
    /// <returns>Lowercase hexadecimal SHA-256 hash string.</returns>
    public static string Sha256Hex(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);

#if NET8_0_OR_GREATER
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
#else
        byte[] hash;
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            hash = sha256.ComputeHash(bytes);
        }
#endif

        return BytesToHexStringLower(hash);
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
}

}
