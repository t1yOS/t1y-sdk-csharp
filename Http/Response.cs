using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace T1YOS.Sdk.Http
{

/// <summary>
/// Response handling utilities for the T1YOS SDK.
/// Handles JSON parsing, safe-mode decryption, timestamp formatting,
/// and error extraction from HTTP responses.
/// </summary>
internal static class ResponseHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Processes an HTTP response and returns a typed API response.
    /// Handles safe-mode decryption, timestamp formatting, and error extraction.
    /// </summary>
    /// <typeparam name="T">Expected type of the response data.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="clientConfig">Client configuration (for decryption keys and time format).</param>
    /// <returns>The deserialized API response.</returns>
    public static async Task<Types.ApiResponse<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        Types.T1YOSInternalConfig clientConfig)
    {
        var content = await response.Content.ReadAsStringAsync();

        // If safe mode is enabled, check if the response is encrypted
        if (clientConfig.IsSafeMode)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("j", out _))
                {
                    // Response is encrypted — decrypt it
                    var keyBytes = Encoding.UTF8.GetBytes(clientConfig.SecretKey);
                    content = Crypto.AESHelper.DecryptAESGCM(content, keyBytes);
                }
            }
            catch (JsonException)
            {
                // Not valid JSON — proceed with raw content
            }
            catch (FormatException)
            {
                // Malformed AES-GCM payload — proceed with raw content
            }
            catch (ArgumentException)
            {
                // Invalid base64 or key — proceed with raw content
            }
            catch (Exception ex) when (!(ex is Utils.T1YError))
            {
                // BouncyCastle exceptions (InvalidCiphertextException, etc.) — proceed with raw content
            }
        }

        // If the response was not successful, throw a T1YError
        if (!response.IsSuccessStatusCode)
        {
            throw ExtractError(content, (int)response.StatusCode);
        }

        // Parse the API response
        Types.ApiResponse<T>? apiResponse;
        try
        {
            apiResponse = JsonSerializer.Deserialize<Types.ApiResponse<T>>(
                content, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new Utils.T1YError(
                (int)response.StatusCode,
                $"Failed to parse response: {ex.Message}");
        }

        if (apiResponse == null)
        {
            throw new Utils.T1YError(
                (int)response.StatusCode,
                "Empty response from server.");
        }

        // Format timestamps in the data payload
        if (apiResponse.Data != null)
        {
            apiResponse.Data = (T)Utils.TimeHelper.FormatTimestampsToLocal(
                apiResponse.Data, clientConfig.TimeFormat)!;
        }

        return apiResponse;
    }

    /// <summary>
    /// Extracts a <see cref="Utils.T1YError"/> from a server error response.
    /// </summary>
    private static Utils.T1YError ExtractError(string content, int statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var code = statusCode;
            if (root.TryGetProperty("code", out var codeEl) && codeEl.TryGetInt32(out var c))
            {
                code = c;
            }

            var message = string.Empty;
            if (root.TryGetProperty("message", out var msgEl))
            {
                message = msgEl.GetString() ?? string.Empty;
            }

            object? data = null;
            if (root.TryGetProperty("data", out var dataEl))
            {
                data = JsonElementToObject(dataEl);
            }

            return new Utils.T1YError(code, message, data);
        }
        catch (JsonException)
        {
            return new Utils.T1YError(statusCode, content);
        }
    }

    /// <summary>
    /// Converts a <see cref="JsonElement"/> to a CLR object
    /// (dictionary, array, or primitive).
    /// </summary>
    internal static object? JsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new System.Collections.Generic.Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = JsonElementToObject(prop.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new object?[element.GetArrayLength()];
                int i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    list[i++] = JsonElementToObject(item);
                }
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var l)) return l;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return null;
        }
    }

    /// <summary>
    /// Handles HTTP client exceptions and wraps them in <see cref="Utils.T1YError"/>.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    /// <returns>A <see cref="Utils.T1YError"/> wrapping the original exception.</returns>
    public static Utils.T1YError HandleHttpError(Exception ex)
    {
        if (ex is Utils.T1YError t1yError)
        {
            return t1yError;
        }

        if (ex is TaskCanceledException || ex is OperationCanceledException)
        {
            return new Utils.T1YError(408, "Request timeout.");
        }

        if (ex is HttpRequestException httpEx)
        {
            return new Utils.T1YError(0, $"Network error: {httpEx.Message}");
        }

        return new Utils.T1YError(0, ex.Message);
    }
}

}
