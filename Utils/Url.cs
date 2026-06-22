using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace T1YOS.Sdk.Utils
{

/// <summary>
/// URL utility functions for the T1YOS SDK.
/// </summary>
public static class UrlHelper
{
    /// <summary>
    /// Normalizes a base URL by removing trailing slashes.
    /// </summary>
    /// <param name="baseUrl">The base URL to normalize.</param>
    /// <returns>Normalized base URL without trailing slashes.</returns>
    public static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            return string.Empty;
        }

        return baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Appends query parameters to a URI builder.
    /// Null values are skipped; non-string values are serialized as JSON.
    /// </summary>
    /// <param name="uriBuilder">The URI builder to modify.</param>
    /// <param name="queryParams">Dictionary of query parameters.</param>
    public static void AppendQueryParams(
        System.UriBuilder uriBuilder,
        Dictionary<string, object?>? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            sb.Append(uriBuilder.Query.TrimStart('?'));
        }

        foreach (var kvp in queryParams)
        {
            if (kvp.Value == null)
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            var encodedKey = Uri.EscapeDataString(kvp.Key);
            var valueStr = kvp.Value is string strVal
                ? strVal
                : JsonSerializer.Serialize(kvp.Value);

            sb.Append(encodedKey);
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(valueStr));
        }

        uriBuilder.Query = sb.ToString();
    }
}

}
