namespace T1YOS.Sdk.Types
{

/// <summary>
/// Configuration options for initializing a T1YOS client.
/// </summary>
public class T1YOSConfig
{
    /// <summary>
    /// Base URL of the t1yOS server. Defaults to <c>https://myapp.t1y.net</c>.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Application ID (must be >= 1001).
    /// </summary>
    public int AppId { get; set; }

    /// <summary>
    /// API key (must be exactly 32 characters).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Secret key for request signing (must be exactly 32 characters).
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// API version. Defaults to 0.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Whether to enable safe mode (AES-256-GCM payload encryption). Defaults to <c>false</c>.
    /// </summary>
    public bool? IsSafeMode { get; set; }

    /// <summary>
    /// Time format for timestamp localization. Defaults to <c>"YYYY-MM-DD HH:mm:ss"</c>.
    /// </summary>
    public string? TimeFormat { get; set; }

    /// <summary>
    /// Time offset in seconds for request signing. Defaults to 0.
    /// </summary>
    public int? Offset { get; set; }
}

/// <summary>
/// Internal configuration with all defaults resolved.
/// </summary>
public class T1YOSInternalConfig
{
    /// <summary>Resolved base URL (no trailing slash).</summary>
    public string BaseUrl { get; set; } = Constants.DefaultBaseUrl;

    /// <summary>Validated application ID.</summary>
    public int AppId { get; set; }

    /// <summary>Validated API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Validated secret key.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Resolved API version.</summary>
    public int Version { get; set; } = Constants.DefaultVersion;

    /// <summary>Resolved safe mode flag.</summary>
    public bool IsSafeMode { get; set; } = Constants.DefaultSafeMode;

    /// <summary>Resolved time format string.</summary>
    public string TimeFormat { get; set; } = Constants.DefaultTimeFormat;

    /// <summary>Resolved time offset in seconds.</summary>
    public int Offset { get; set; } = Constants.DefaultOffset;
}

/// <summary>
/// Supported HTTP methods for API requests.
/// </summary>
public enum HttpMethod
{
    /// <summary>HTTP GET</summary>
    GET,

    /// <summary>HTTP POST</summary>
    POST,

    /// <summary>HTTP PUT</summary>
    PUT,

    /// <summary>HTTP DELETE</summary>
    DELETE
}
}

