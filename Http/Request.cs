using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace T1YOS.Sdk.Http
{

/// <summary>
/// Options for executing an HTTP request through the T1YOS API.
/// </summary>
internal class RequestOptions
{
    /// <summary>HTTP method.</summary>
    public Types.HttpMethod Method { get; set; }

    /// <summary>API path (e.g., <c>/v5/classes/Users</c>).</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Request body or query parameters.</summary>
    public object? Params { get; set; }

    /// <summary>Whether to force encryption. Null means use client default.</summary>
    public bool? Encryption { get; set; }

    /// <summary>Request timeout in milliseconds. Null means use default (5 min).</summary>
    public int? Timeout { get; set; }
}

/// <summary>
/// HTTP request execution engine for the T1YOS SDK.
/// Handles URL construction, body serialization, encryption, signing,
/// and header injection for all API requests.
/// </summary>
internal static class RequestExecutor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Executes an HTTP request and returns the typed API response.
    /// This is the core method that all client operations delegate to.
    /// </summary>
    /// <typeparam name="T">Expected type of the response data.</typeparam>
    /// <param name="clientConfig">The client configuration.</param>
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <param name="options">Request options.</param>
    /// <returns>The deserialized API response.</returns>
    public static async Task<Types.ApiResponse<T>> ExecuteRequestAsync<T>(
        Types.T1YOSInternalConfig clientConfig,
        HttpClient httpClient,
        RequestOptions options)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        // 1. Normalize base URL
        var baseUrl = Utils.UrlHelper.NormalizeBaseUrl(clientConfig.BaseUrl);

        // 2. Convert date types in params
        var convertedParams = options.Params != null
            ? Utils.ConvertHelper.ConvertDateTypes(options.Params)
            : null;

        // 3. Build URL
        var fullUrl = baseUrl + options.Path;
        var uriBuilder = new UriBuilder(fullUrl);

        string bodyJson = string.Empty;
        var shouldEncrypt = options.Encryption ?? clientConfig.IsSafeMode;

        if (options.Method == Types.HttpMethod.GET)
        {
            // GET: append params as query string
            if (convertedParams is Dictionary<string, object?> queryDict)
            {
                Utils.UrlHelper.AppendQueryParams(uriBuilder, queryDict);
            }
        }
        else
        {
            // Non-GET: serialize body to JSON
            if (convertedParams != null)
            {
                bodyJson = JsonSerializer.Serialize(convertedParams, SerializerOptions);

                // Encrypt the body if safe mode is enabled
                if (shouldEncrypt)
                {
                    var keyBytes = Encoding.UTF8.GetBytes(clientConfig.SecretKey);
                    bodyJson = Crypto.AESHelper.EncryptAESGCM(bodyJson, keyBytes);
                }
            }
        }

        // 4. Extract path and query for signing
        var pathAndQuery = uriBuilder.Path;
        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            pathAndQuery += uriBuilder.Query;
        }

        // 5. Compute safe timestamp and signature
        var timestamp = Crypto.SignHelper.GetSafeTimestamp(clientConfig.Offset);
        var signatureInput = new Crypto.SignatureInput
        {
            Method = options.Method.ToString().ToUpperInvariant(),
            PathAndQuery = pathAndQuery,
            Body = bodyJson,
            AppId = clientConfig.AppId,
            Timestamp = long.Parse(timestamp),
            SecretKey = clientConfig.SecretKey
        };
        var signature = Crypto.SignHelper.CreateSignature(signatureInput);

        // 6. Build HTTP request
        var httpMethod = new System.Net.Http.HttpMethod(options.Method.ToString().ToUpperInvariant());
        var request = new HttpRequestMessage(httpMethod, uriBuilder.Uri);

        // 7. Set auth headers
        request.Headers.Add("X-T1Y-Application-ID", clientConfig.AppId.ToString());
        request.Headers.Add("X-T1Y-API-Key", clientConfig.ApiKey);
        request.Headers.Add("X-T1Y-Safe-Timestamp", timestamp);
        request.Headers.Add("X-T1Y-Safe-Sign", signature);

        // 8. Set body for non-GET requests
        if (options.Method != Types.HttpMethod.GET && !string.IsNullOrEmpty(bodyJson))
        {
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
        }

        // 9. Send request with timeout
        try
        {
            var timeoutMs = options.Timeout ?? Constants.RequestTimeoutMs;
            using var cts = new CancellationTokenSource(timeoutMs);

            var response = await httpClient.SendAsync(request, cts.Token);

            return await ResponseHandler.HandleResponseAsync<T>(response, clientConfig);
        }
        catch (Utils.T1YError)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ResponseHandler.HandleHttpError(ex);
        }
    }
}

}
