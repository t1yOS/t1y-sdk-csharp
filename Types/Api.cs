using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace T1YOS.Sdk.Types
{

/// <summary>
/// Standard API response envelope returned by the t1yOS server.
/// </summary>
/// <typeparam name="T">Type of the response data payload.</typeparam>
public class ApiResponse<T>
{
    /// <summary>HTTP status code or error code.</summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>Response message from the server.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Response data payload.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

/// <summary>
/// Result returned after inserting a single document.
/// </summary>
public class InsertResult
{
    /// <summary>The ObjectID of the newly inserted document.</summary>
    [JsonPropertyName("objectId")]
    public string? ObjectId { get; set; }
}

/// <summary>
/// Result returned after inserting multiple documents.
/// </summary>
public class InsertManyResult
{
    /// <summary>Array of ObjectIDs for the inserted documents.</summary>
    [JsonPropertyName("objectIds")]
    public string[]? ObjectIds { get; set; }

    /// <summary>Number of documents inserted.</summary>
    [JsonPropertyName("insertedCount")]
    public int InsertedCount { get; set; }
}

/// <summary>
/// Result returned after deleting a single document by ID.
/// </summary>
public class DeleteResult
{
    /// <summary>Number of documents deleted (0 or 1).</summary>
    [JsonPropertyName("deletedCount")]
    public int DeletedCount { get; set; }
}

/// <summary>
/// Result returned after deleting multiple documents.
/// </summary>
public class DeleteManyResult
{
    /// <summary>Number of documents deleted.</summary>
    [JsonPropertyName("deletedCount")]
    public int DeletedCount { get; set; }
}

/// <summary>
/// Result returned after updating a single document.
/// </summary>
public class UpdateResult
{
    /// <summary>Number of documents modified (0 or 1).</summary>
    [JsonPropertyName("modifiedCount")]
    public int ModifiedCount { get; set; }
}

/// <summary>
/// Result returned after updating multiple documents.
/// </summary>
public class UpdateManyResult
{
    /// <summary>Number of documents modified.</summary>
    [JsonPropertyName("modifiedCount")]
    public int ModifiedCount { get; set; }
}

/// <summary>
/// Result returned when finding a single document.
/// </summary>
public class FindResult
{
    /// <summary>The found document as a dictionary, or null if not found.</summary>
    [JsonPropertyName("result")]
    public Dictionary<string, object?>? Result { get; set; }
}

/// <summary>
/// Pagination metadata.
/// </summary>
public class Pagination
{
    /// <summary>Total number of items across all pages.</summary>
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    /// <summary>Total number of pages.</summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

/// <summary>
/// Paginated query result.
/// </summary>
public class PaginationResult
{
    /// <summary>Array of result documents.</summary>
    [JsonPropertyName("results")]
    public Dictionary<string, object?>[]? Results { get; set; }

    /// <summary>Current page number.</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>Number of items per page.</summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>Pagination metadata.</summary>
    [JsonPropertyName("pagination")]
    public Pagination? Pagination { get; set; }
}

/// <summary>
/// Aggregation pipeline result.
/// </summary>
public class AggregateResult
{
    /// <summary>Array of aggregated result documents.</summary>
    [JsonPropertyName("results")]
    public Dictionary<string, object?>[]? Results { get; set; }
}

/// <summary>
/// Result returned from the initialization endpoint.
/// </summary>
public class InitResult
{
    /// <summary>Server's current Unix timestamp in seconds.</summary>
    [JsonPropertyName("unix")]
    public long Unix { get; set; }

    /// <summary>Whether the server has safe mode enabled.</summary>
    [JsonPropertyName("is_safe_mode")]
    public bool IsSafeMode { get; set; }
}
}

