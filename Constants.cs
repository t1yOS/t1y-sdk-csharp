namespace T1YOS.Sdk
{

/// <summary>
/// Global constants for the T1YOS SDK.
/// </summary>
public static class Constants
{
    /// <summary>Default base URL for the t1yOS platform.</summary>
    public const string DefaultBaseUrl = "https://myapp.t1y.net";

    /// <summary>Minimum valid application ID.</summary>
    public const int MinAppId = 1001;

    /// <summary>Required length for API keys (32 characters).</summary>
    public const int ApiKeyLength = 32;

    /// <summary>Required length for secret keys (32 characters).</summary>
    public const int SecretKeyLength = 32;

    /// <summary>Default API version.</summary>
    public const int DefaultVersion = 0;

    /// <summary>Default time format string for timestamp localization.</summary>
    public const string DefaultTimeFormat = "YYYY-MM-DD HH:mm:ss";

    /// <summary>Default time offset in seconds.</summary>
    public const int DefaultOffset = 0;

    /// <summary>Default safe mode setting.</summary>
    public const bool DefaultSafeMode = false;

    /// <summary>Maximum allowed time difference for request signatures (seconds).</summary>
    public const int MaxTimeDiff = 10;

    /// <summary>Default request timeout in milliseconds (5 minutes).</summary>
    public const int RequestTimeoutMs = 5 * 60 * 1000;

    /// <summary>Maximum page size for paginated queries.</summary>
    public const int MaxPageSize = 100;

    /// <summary>Default page size for paginated queries.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>Required length for ObjectID hex strings.</summary>
    public const int ObjectIdLength = 24;

    /// <summary>API version prefix for REST endpoints.</summary>
    public const string ApiVersion = "v5";
}
}

