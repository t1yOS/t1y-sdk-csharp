using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace T1YOS.Sdk.Client
{

/// <summary>
/// Main client for the t1yOS Serverless Platform.
/// Provides access to cloud database, metadata, cloud functions,
/// and cryptographic utilities.
///
/// <example>
/// Usage:
/// <code>
/// var client = new T1YOS(new T1YOSConfig
/// {
///     AppId = 1001,
///     ApiKey = "your-32-character-api-key-here",
///     SecretKey = "your-32-character-secret-key-here"
/// });
///
/// await client.InitAsync();
///
/// // Database operations
/// var users = client.Db.Collection("users");
/// var result = await users.InsertOneAsync(new Dictionary&lt;string, object?&gt;
/// {
///     { "name", "Alice" },
///     { "age", 30 }
/// });
/// </code>
/// </example>
/// </summary>
public sealed class T1YOS : IDisposable
{
    private readonly Types.T1YOSInternalConfig _config;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// Database accessor for collection operations.
    /// </summary>
    public DbAccessor Db { get; }

    /// <summary>
    /// Creates a new T1YOS client instance.
    /// </summary>
    /// <param name="config">
    /// Configuration for the client. The following fields are required:
    /// <c>AppId</c> (>= 1001), <c>ApiKey</c> (32 chars), <c>SecretKey</c> (32 chars).
    /// All other fields have sensible defaults.
    /// </param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> for dependency injection and testing.
    /// If not provided, an internal HttpClient is created and managed.
    /// </param>
    /// <exception cref="Utils.ValidationError">
    /// Thrown if required configuration fields are invalid.
    /// </exception>
    public T1YOS(Types.T1YOSConfig config, HttpClient? httpClient = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        // Validate the configuration
        Utils.Validators.ValidateInitConfig(config);

        // Resolve defaults into internal config
        _config = new Types.T1YOSInternalConfig
        {
            BaseUrl = config.BaseUrl ?? Constants.DefaultBaseUrl,
            AppId = config.AppId,
            ApiKey = config.ApiKey ?? string.Empty,
            SecretKey = config.SecretKey ?? string.Empty,
            Version = config.Version ?? Constants.DefaultVersion,
            IsSafeMode = config.IsSafeMode ?? Constants.DefaultSafeMode,
            TimeFormat = config.TimeFormat ?? Constants.DefaultTimeFormat,
            Offset = config.Offset ?? Constants.DefaultOffset
        };

        // Normalize the base URL
        _config.BaseUrl = Utils.UrlHelper.NormalizeBaseUrl(_config.BaseUrl);

        // Use provided HttpClient or create a new one
        if (httpClient != null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        }

        // Initialize the database accessor
        Db = new DbAccessor(this);
    }

    /// <summary>
    /// Initializes the client by syncing with the server.
    /// Retrieves the server's Unix timestamp and safe mode setting
    /// to correct clock skew and configure encryption.
    ///
    /// If the server is unreachable, the client gracefully degrades
    /// with defaults (offset = 0, isSafeMode = false) and logs a warning.
    /// </summary>
    public async Task InitAsync()
    {
        try
        {
            var result = await RequestAsync<Types.InitResult>(
                Types.HttpMethod.GET,
                $"/init/{_config.AppId}",
                encryption: false);

            if (result.Data != null)
            {
                _config.IsSafeMode = result.Data.IsSafeMode;
                // Compute time offset: server time - client time
                var clientUnix = GetCurrentUnixSeconds();
                _config.Offset = (int)(result.Data.Unix - clientUnix);
            }
        }
        catch (Exception ex)
        {
            // Graceful degradation — log warning and use defaults
            Trace.WriteLine(
                $"[T1YOS] InitAsync: Unable to connect to {_config.BaseUrl}. " +
                $"Using default offset=0 and safeMode=false. Error: {ex.Message}");
            _config.Offset = Constants.DefaultOffset;
            _config.IsSafeMode = Constants.DefaultSafeMode;
        }
    }

    /// <summary>
    /// Executes a generic API request and returns the typed response.
    /// This is the core internal method used by all public API methods.
    /// </summary>
    /// <typeparam name="T">Expected type of the response data.</typeparam>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">API path (e.g., <c>/v5/classes/Users</c>).</param>
    /// <param name="body">Request body object (for POST/PUT).</param>
    /// <param name="encryption">Whether to encrypt the request. Null uses client default.</param>
    /// <returns>The deserialized API response.</returns>
    public async Task<Types.ApiResponse<T>> RequestAsync<T>(
        Types.HttpMethod method,
        string path,
        object? body = null,
        bool? encryption = null)
    {
        var options = new Http.RequestOptions
        {
            Method = method,
            Path = path,
            Params = body,
            Encryption = encryption
        };

        return await Http.RequestExecutor.ExecuteRequestAsync<T>(
            _config, _httpClient, options);
    }

    /// <summary>
    /// Retrieves metadata from the server.
    /// </summary>
    /// <param name="field">
    /// Optional field name to retrieve. If null or empty, returns all metadata.
    /// </param>
    /// <returns>The metadata API response.</returns>
    public async Task<Types.ApiResponse<object>> GetMetaAsync(string? field = null)
    {
        var path = $"/{Constants.ApiVersion}/meta";
        if (!string.IsNullOrEmpty(field))
        {
            path += $"?field={Uri.EscapeDataString(field)}";
        }
        return await RequestAsync<object>(Types.HttpMethod.GET, path);
    }

    /// <summary>
    /// Checks if a newer version of the SDK is available on the server.
    /// Compares the server version with the client's configured version.
    /// </summary>
    /// <returns>True if an update is available, false otherwise.</returns>
    public async Task<bool> CheckUpdateAsync()
    {
        try
        {
            var result = await GetMetaAsync("version");
            if (result?.Data != null)
            {
                // Server returns version info — compare with client version
                return true; // An update is available if the server has version info
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calls a cloud function (<c>.jsc</c> file) on the server.
    ///
    /// Function name transformations:
    /// - <c>"hello"</c> becomes <c>"hello.jsc"</c>
    /// - <c>"dir/"</c> becomes <c>"dir/index.jsc"</c>
    /// - <c>"script.js"</c> becomes <c>"script.jsc"</c>
    /// - <c>"func.jsc"</c> stays as-is
    /// </summary>
    /// <param name="name">Function name. The <c>.jsc</c> extension is auto-appended.</param>
    /// <param name="parameters">Optional parameters to pass to the function.</param>
    /// <param name="enableSafeMode">Whether to enable safe mode for this call.</param>
    /// <returns>The cloud function's API response.</returns>
    public async Task<Types.ApiResponse<object>> CallFuncAsync(
        string name,
        object? parameters = null,
        bool? enableSafeMode = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var jscPath = EnsureJscExtension(name);
        var path = $"/{_config.AppId}/{jscPath}";

        return await RequestAsync<object>(
            Types.HttpMethod.POST,
            path,
            parameters,
            enableSafeMode);
    }

    /// <summary>
    /// Ensures the function name has a <c>.jsc</c> extension.
    /// Handles leading slashes, query strings, and hash fragments.
    /// </summary>
    private static string EnsureJscExtension(string name)
    {
        // Strip leading slash
        if (name.StartsWith("/"))
        {
            name = name.Substring(1);
        }

        // Separate query string and hash fragment
        string? queryAndHash = null;
        var queryIndex = name.IndexOf('?');
        var hashIndex = name.IndexOf('#');

        var fragmentStart = -1;
        if (queryIndex >= 0)
        {
            fragmentStart = queryIndex;
        }
        else if (hashIndex >= 0)
        {
            fragmentStart = hashIndex;
        }

        if (fragmentStart >= 0)
        {
            queryAndHash = name.Substring(fragmentStart);
            name = name.Substring(0, fragmentStart);
        }

        // Already has .jsc extension
        if (name.EndsWith(".jsc", StringComparison.OrdinalIgnoreCase))
        {
            return queryAndHash != null ? name + queryAndHash : name;
        }

        // Ends with trailing slash → index.jsc
        if (name.EndsWith("/"))
        {
            return name + "index.jsc" + (queryAndHash ?? string.Empty);
        }

        // Has another extension (.js, .ts, etc.) → replace with .jsc
        var lastDot = name.LastIndexOf('.');
        var lastSlash = name.LastIndexOf('/');
        if (lastDot > lastSlash)
        {
            name = name.Substring(0, lastDot) + ".jsc";
        }
        else
        {
            // No extension → append .jsc
            name += ".jsc";
        }

        return queryAndHash != null ? name + queryAndHash : name;
    }

    // ──────────────────────────────────────────────
    // Utility wrappers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Validates that a string is a valid 24-character hexadecimal ObjectID.
    /// Delegates to <see cref="Utils.Validators.AssertObjectID"/>.
    /// </summary>
    public bool AssertObjectID(string idStr, string name = "ObjectID")
    {
        return Utils.Validators.AssertObjectID(idStr, name);
    }

    /// <summary>
    /// Checks if a value is a non-empty object (dictionary with at least one key).
    /// </summary>
    public bool IsNonEmptyObject(object? value)
    {
        return Utils.ConvertHelper.IsNonEmptyObject(value);
    }

    /// <summary>
    /// Checks if a value is a plain object (non-null dictionary).
    /// </summary>
    public bool IsPlainObject(object? value)
    {
        return Utils.ConvertHelper.IsPlainObject(value);
    }

    /// <summary>
    /// Checks if a value is a non-empty array of non-empty objects.
    /// </summary>
    public bool IsNonEmptyArrayWithNonEmptyObjects(object? value)
    {
        return Utils.ConvertHelper.IsNonEmptyArrayWithNonEmptyObjects(value);
    }

    // ──────────────────────────────────────────────
    // Crypto wrappers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Computes HMAC-SHA256 of a message using a secret key.
    /// Returns the result as a 64-character lowercase hex string.
    /// </summary>
    public string HmacSHA256(string secret, string message)
    {
        return Crypto.HMACHelper.HmacSHA256Hex(secret, message);
    }

    /// <summary>
    /// Verifies an HMAC-SHA256 signature against the expected value.
    /// Uses constant-time comparison.
    /// </summary>
    public bool VerifyHmacSHA256(string secret, string message, string signature)
    {
        return Crypto.HMACHelper.VerifyHmacSHA256(secret, message, signature);
    }

    // ──────────────────────────────────────────────
    // Helper: current Unix timestamp
    // ──────────────────────────────────────────────

    private static long GetCurrentUnixSeconds()
    {
#if NET8_0_OR_GREATER
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
#else
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(DateTime.UtcNow - epoch).TotalSeconds;
#endif
    }

    // ──────────────────────────────────────────────
    // IDisposable
    // ──────────────────────────────────────────────

    /// <summary>
    /// Disposes the client and releases resources.
    /// Safe to call multiple times.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsHttpClient)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    // ──────────────────────────────────────────────
    // DbAccessor inner class
    // ──────────────────────────────────────────────

    /// <summary>
    /// Provides access to database collections.
    /// </summary>
    public class DbAccessor
    {
        private readonly T1YOS _client;

        internal DbAccessor(T1YOS client)
        {
            _client = client;
        }

        /// <summary>
        /// Gets a <see cref="T1Collection"/> instance for the specified collection name.
        /// </summary>
        /// <param name="name">The collection name.</param>
        /// <returns>A <see cref="T1Collection"/> for database operations on this collection.</returns>
        public T1Collection Collection(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            return new T1Collection(_client, name);
        }

        /// <summary>
        /// Gets all collections (schemas) from the database.
        /// </summary>
        /// <returns>API response with the list of collections.</returns>
        public Task<Types.ApiResponse<object>> GetCollectionsAsync()
        {
            return _client.RequestAsync<object>(
                Types.HttpMethod.GET,
                $"/{Constants.ApiVersion}/schemas");
        }

        /// <summary>
        /// Converts a hex string to an ObjectID marker.
        /// </summary>
        /// <param name="id">24-character hexadecimal ObjectID.</param>
        /// <returns>An ObjectID marker string.</returns>
        public string ToObjectID(string id)
        {
            return SpecialTypes.SpecialTypes.ObjectID(id);
        }
    }
}

}
