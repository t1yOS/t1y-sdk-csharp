using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace T1YOS.Sdk.Crypto
{

/// <summary>
/// Represents an AES-256-GCM encrypted payload as exchanged with the t1yOS server.
/// </summary>
public class AESGCMPayload
{
    /// <summary>Base64-encoded nonce (12 bytes).</summary>
    [JsonPropertyName("n")]
    public string N { get; set; } = string.Empty;

    /// <summary>Base64-encoded ciphertext.</summary>
    [JsonPropertyName("j")]
    public string J { get; set; } = string.Empty;

    /// <summary>Base64-encoded authentication tag (16 bytes).</summary>
    [JsonPropertyName("t")]
    public string T { get; set; } = string.Empty;
}

/// <summary>
/// AES-256-GCM encryption/decryption utilities for the T1YOS SDK.
/// Used for payload encryption when safe mode is enabled.
/// Uses BouncyCastle <see cref="GcmBlockCipher"/> with <see cref="AesEngine"/>
/// across all target frameworks for consistent behavior.
/// </summary>
public static class AESHelper
{
    /// <summary>GCM authentication tag length in bits.</summary>
    private const int GcmMacSizeBits = 128;

    /// <summary>Nonce length in bytes.</summary>
    private const int NonceLengthBytes = 12;

    /// <summary>Required AES key length in bytes (256 bits).</summary>
    private const int KeyLengthBytes = 32;

    /// <summary>
    /// Encrypts a string using AES-256-GCM.
    /// </summary>
    /// <param name="data">The plaintext string to encrypt.</param>
    /// <param name="key">The 32-byte (256-bit) encryption key.</param>
    /// <returns>A JSON string containing base64-encoded nonce, ciphertext, and tag.</returns>
    /// <exception cref="ArgumentException">Thrown if the key is not exactly 32 bytes.</exception>
    public static string EncryptAESGCM(string data, byte[] key)
    {
        if (key == null || key.Length != KeyLengthBytes)
        {
            throw new ArgumentException(
                $"Key must be exactly {KeyLengthBytes} bytes (256 bits).", nameof(key));
        }

        var plaintextBytes = Encoding.UTF8.GetBytes(data);

        // Generate random nonce
        var nonce = new byte[NonceLengthBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(nonce);
        }

        // Initialize GCM cipher with AesEngine
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(
            new KeyParameter(key),
            GcmMacSizeBits,
            nonce);

        cipher.Init(true, parameters);

        // Encrypt
        var ciphertext = new byte[cipher.GetOutputSize(plaintextBytes.Length)];
        var len = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertext, 0);
        cipher.DoFinal(ciphertext, len);

        // Split: all but last 16 bytes are ciphertext, last 16 bytes are tag
        var tagLen = GcmMacSizeBits / 8; // 16 bytes
        var actualCiphertext = new byte[ciphertext.Length - tagLen];
        var tag = new byte[tagLen];

        Array.Copy(ciphertext, 0, actualCiphertext, 0, actualCiphertext.Length);
        Array.Copy(ciphertext, actualCiphertext.Length, tag, 0, tagLen);

        // Create payload
        var payload = new AESGCMPayload
        {
            N = Convert.ToBase64String(nonce),
            J = Convert.ToBase64String(actualCiphertext),
            T = Convert.ToBase64String(tag)
        };

        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Decrypts an AES-256-GCM encrypted JSON payload.
    /// </summary>
    /// <param name="jsonPayload">
    /// The JSON string containing <c>n</c> (nonce), <c>j</c> (ciphertext),
    /// and <c>t</c> (tag) fields, all base64-encoded.
    /// </param>
    /// <param name="key">The 32-byte (256-bit) decryption key.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException">Thrown if the key is not exactly 32 bytes.</exception>
    /// <exception cref="FormatException">Thrown if the JSON payload is malformed.</exception>
    public static string DecryptAESGCM(string jsonPayload, byte[] key)
    {
        if (key == null || key.Length != KeyLengthBytes)
        {
            throw new ArgumentException(
                $"Key must be exactly {KeyLengthBytes} bytes (256 bits).", nameof(key));
        }

        // Parse the JSON payload
        var payload = JsonSerializer.Deserialize<AESGCMPayload>(jsonPayload);
        if (payload == null ||
            string.IsNullOrEmpty(payload.N) ||
            string.IsNullOrEmpty(payload.J) ||
            string.IsNullOrEmpty(payload.T))
        {
            throw new FormatException(
                "Invalid AES-GCM payload: must contain 'n', 'j', and 't' fields.");
        }

        // Base64 decode
        var nonce = Convert.FromBase64String(payload.N);
        var ciphertext = Convert.FromBase64String(payload.J);
        var tag = Convert.FromBase64String(payload.T);

        if (nonce.Length != NonceLengthBytes)
        {
            throw new FormatException(
                $"Invalid nonce length: expected {NonceLengthBytes}, got {nonce.Length}.");
        }

        // Concatenate ciphertext + tag for BouncyCastle
        var combined = new byte[ciphertext.Length + tag.Length];
        Array.Copy(ciphertext, 0, combined, 0, ciphertext.Length);
        Array.Copy(tag, 0, combined, ciphertext.Length, tag.Length);

        // Initialize GCM cipher for decryption
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(
            new KeyParameter(key),
            GcmMacSizeBits,
            nonce);

        cipher.Init(false, parameters);

        // Decrypt: ProcessBytes + DoFinal gives the actual plaintext length
        var plaintext = new byte[cipher.GetOutputSize(combined.Length)];
        var len = cipher.ProcessBytes(combined, 0, combined.Length, plaintext, 0);
        var finalLen = cipher.DoFinal(plaintext, len);
        var totalLen = len + finalLen;

        return Encoding.UTF8.GetString(plaintext, 0, totalLen);
    }
}

}
