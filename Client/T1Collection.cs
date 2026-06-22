using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace T1YOS.Sdk.Client
{

/// <summary>
/// Represents a database collection in the t1yOS cloud database.
/// Provides CRUD operations, advanced queries, and schema management.
///
/// All methods delegate to <see cref="T1YOS.RequestAsync{T}"/> with the
/// appropriate HTTP method and REST path.
/// </summary>
public class T1Collection
{
    private readonly T1YOS _client;
    private readonly string _name;
    private readonly string _encodedName;

    /// <summary>
    /// Creates a new T1Collection instance.
    /// </summary>
    /// <param name="client">The parent T1YOS client.</param>
    /// <param name="name">The collection name.</param>
    public T1Collection(T1YOS client, string name)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _encodedName = Uri.EscapeDataString(_name);
    }

    /// <summary>Gets the collection name.</summary>
    public string Name => _name;

    // ──────────────────────────────────────────────
    // Single Document Operations
    // ──────────────────────────────────────────────

    /// <summary>
    /// Inserts a single document into the collection.
    /// </summary>
    /// <param name="data">The document data as a dictionary. Must be non-empty.</param>
    /// <returns>API response with the new document's ObjectID.</returns>
    public Task<Types.ApiResponse<Types.InsertResult>> InsertOneAsync(
        Dictionary<string, object?> data)
    {
        if (data == null || data.Count == 0)
        {
            throw new ArgumentException("Data must be a non-empty object.", nameof(data));
        }

        return _client.RequestAsync<Types.InsertResult>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}",
            data);
    }

    /// <summary>
    /// Finds a document by its ObjectID.
    /// </summary>
    /// <param name="objectId">The 24-character hexadecimal ObjectID.</param>
    /// <returns>API response with the found document.</returns>
    public Task<Types.ApiResponse<Types.FindResult>> FindByIdAsync(string objectId)
    {
        Utils.Validators.AssertObjectID(objectId);
        var encodedId = Uri.EscapeDataString(objectId);

        return _client.RequestAsync<Types.FindResult>(
            Types.HttpMethod.GET,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/{encodedId}");
    }

    /// <summary>
    /// Updates a document by its ObjectID.
    /// </summary>
    /// <param name="objectId">The 24-character hexadecimal ObjectID.</param>
    /// <param name="data">The update data. Must be non-empty.</param>
    /// <returns>API response with the modification count.</returns>
    public Task<Types.ApiResponse<Types.UpdateResult>> UpdateByIdAsync(
        string objectId, Dictionary<string, object?> data)
    {
        Utils.Validators.AssertObjectID(objectId);

        if (data == null || data.Count == 0)
        {
            throw new ArgumentException("Data must be a non-empty object.", nameof(data));
        }

        var encodedId = Uri.EscapeDataString(objectId);

        return _client.RequestAsync<Types.UpdateResult>(
            Types.HttpMethod.PUT,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/{encodedId}",
            data);
    }

    /// <summary>
    /// Deletes a document by its ObjectID.
    /// </summary>
    /// <param name="objectId">The 24-character hexadecimal ObjectID.</param>
    /// <returns>API response with the deletion count.</returns>
    public Task<Types.ApiResponse<Types.DeleteResult>> DeleteByIdAsync(string objectId)
    {
        Utils.Validators.AssertObjectID(objectId);
        var encodedId = Uri.EscapeDataString(objectId);

        return _client.RequestAsync<Types.DeleteResult>(
            Types.HttpMethod.DELETE,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/{encodedId}");
    }

    // ──────────────────────────────────────────────
    // Filter-based Operations
    // ──────────────────────────────────────────────

    /// <summary>
    /// Finds a single document matching the filter criteria.
    /// </summary>
    /// <param name="filter">Query filter as a non-empty dictionary.</param>
    /// <returns>API response with the found document.</returns>
    public Task<Types.ApiResponse<Types.FindResult>> FindOneAsync(
        Dictionary<string, object?> filter)
    {
        if (filter == null || filter.Count == 0)
        {
            throw new ArgumentException("Filter must be a non-empty object.", nameof(filter));
        }

        return _client.RequestAsync<Types.FindResult>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/one",
            filter);
    }

    /// <summary>
    /// Updates a single document matching the filter criteria.
    /// </summary>
    /// <param name="filter">Query filter to match documents. Must be non-empty.</param>
    /// <param name="body">Update operations (e.g., <c>$set</c>, <c>$inc</c>). Must be non-empty.</param>
    /// <returns>API response with the modification count.</returns>
    public Task<Types.ApiResponse<Types.UpdateResult>> UpdateOneAsync(
        Dictionary<string, object?> filter, Dictionary<string, object?> body)
    {
        if (filter == null || filter.Count == 0)
        {
            throw new ArgumentException("Filter must be a non-empty object.", nameof(filter));
        }

        if (body == null || body.Count == 0)
        {
            throw new ArgumentException("Body must be a non-empty object.", nameof(body));
        }

        var payload = new Dictionary<string, object?>
        {
            ["filter"] = filter,
            ["body"] = body
        };

        return _client.RequestAsync<Types.UpdateResult>(
            Types.HttpMethod.PUT,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/one",
            payload);
    }

    /// <summary>
    /// Deletes a single document matching the filter criteria.
    /// </summary>
    /// <param name="filter">Query filter to match documents. Must be non-empty.</param>
    /// <returns>API response with the deletion count.</returns>
    public Task<Types.ApiResponse<Types.DeleteResult>> DeleteOneAsync(
        Dictionary<string, object?> filter)
    {
        if (filter == null || filter.Count == 0)
        {
            throw new ArgumentException("Filter must be a non-empty object.", nameof(filter));
        }

        return _client.RequestAsync<Types.DeleteResult>(
            Types.HttpMethod.DELETE,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/one",
            filter);
    }

    // ──────────────────────────────────────────────
    // Bulk Operations
    // ──────────────────────────────────────────────

    /// <summary>
    /// Inserts multiple documents into the collection.
    /// </summary>
    /// <param name="dataList">Array of document data dictionaries. Must be non-empty.</param>
    /// <returns>API response with inserted ObjectIDs and count.</returns>
    public Task<Types.ApiResponse<Types.InsertManyResult>> InsertManyAsync(
        Dictionary<string, object?>[] dataList)
    {
        if (dataList == null || dataList.Length == 0)
        {
            throw new ArgumentException(
                "Data list must be a non-empty array.", nameof(dataList));
        }

        return _client.RequestAsync<Types.InsertManyResult>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/many",
            dataList);
    }

    /// <summary>
    /// Updates multiple documents matching the filter criteria.
    /// </summary>
    /// <param name="filter">Query filter to match documents. Must be non-empty.</param>
    /// <param name="body">Update operations. Must be non-empty.</param>
    /// <returns>API response with the modification count.</returns>
    public Task<Types.ApiResponse<Types.UpdateManyResult>> UpdateManyAsync(
        Dictionary<string, object?> filter, Dictionary<string, object?> body)
    {
        if (filter == null || filter.Count == 0)
        {
            throw new ArgumentException("Filter must be a non-empty object.", nameof(filter));
        }

        if (body == null || body.Count == 0)
        {
            throw new ArgumentException("Body must be a non-empty object.", nameof(body));
        }

        var payload = new Dictionary<string, object?>
        {
            ["filter"] = filter,
            ["body"] = body
        };

        return _client.RequestAsync<Types.UpdateManyResult>(
            Types.HttpMethod.PUT,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/many",
            payload);
    }

    /// <summary>
    /// Deletes multiple documents matching the filter criteria.
    /// </summary>
    /// <param name="filter">Query filter to match documents. Must be non-empty.</param>
    /// <returns>API response with the deletion count.</returns>
    public Task<Types.ApiResponse<Types.DeleteManyResult>> DeleteManyAsync(
        Dictionary<string, object?> filter)
    {
        if (filter == null || filter.Count == 0)
        {
            throw new ArgumentException("Filter must be a non-empty object.", nameof(filter));
        }

        return _client.RequestAsync<Types.DeleteManyResult>(
            Types.HttpMethod.DELETE,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/many",
            filter);
    }

    // ──────────────────────────────────────────────
    // Advanced Queries
    // ──────────────────────────────────────────────

    /// <summary>
    /// Performs a paginated query on the collection.
    /// </summary>
    /// <param name="page">Page number (1-based). Default: 1. Throws if less than 1.</param>
    /// <param name="size">Number of items per page. Throws if less than 1 or greater than 100.</param>
    /// <param name="sort">Optional sort specification (e.g., <c>{ "createdAt": -1 }</c>).</param>
    /// <param name="filter">Optional query filter.</param>
    /// <returns>API response with paginated results.</returns>
    /// <exception cref="ArgumentException">Thrown if page or size are out of range.</exception>
    public Task<Types.ApiResponse<Types.PaginationResult>> FindAsync(
        int page = 1,
        int size = 10,
        Dictionary<string, object?>? sort = null,
        Dictionary<string, object?>? filter = null)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be >= 1.", nameof(page));
        }

        if (size < 1 || size > Constants.MaxPageSize)
        {
            throw new ArgumentException(
                $"Size must be between 1 and {Constants.MaxPageSize}.", nameof(size));
        }

        var body = new Dictionary<string, object?>
        {
            ["page"] = page,
            ["size"] = size
        };

        if (sort != null)
        {
            body["sort"] = sort;
        }

        if (filter != null)
        {
            body["filter"] = filter;
        }

        return _client.RequestAsync<Types.PaginationResult>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/find",
            body);
    }

    /// <summary>
    /// Executes an aggregation pipeline on the collection.
    /// </summary>
    /// <param name="pipeline">
    /// Array of aggregation pipeline stages. Must be non-empty.
    /// Each stage is a dictionary representing a MongoDB aggregation operation.
    /// </param>
    /// <returns>API response with aggregated results.</returns>
    public Task<Types.ApiResponse<Types.AggregateResult>> AggregateAsync(
        Dictionary<string, object?>[] pipeline)
    {
        if (pipeline == null || pipeline.Length == 0)
        {
            throw new ArgumentException(
                "Pipeline must be a non-empty array.", nameof(pipeline));
        }

        return _client.RequestAsync<Types.AggregateResult>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/aggregate",
            pipeline);
    }

    /// <summary>
    /// Counts documents matching the optional filter.
    /// </summary>
    /// <param name="filter">Optional query filter.</param>
    /// <returns>API response with the document count.</returns>
    public Task<Types.ApiResponse<object>> CountAsync(
        Dictionary<string, object?>? filter = null)
    {
        return _client.RequestAsync<object>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/count",
            filter);
    }

    /// <summary>
    /// Gets distinct values for a specified field.
    /// </summary>
    /// <param name="fieldName">The field name to get distinct values for.</param>
    /// <param name="filter">Optional query filter.</param>
    /// <returns>API response with distinct values.</returns>
    public Task<Types.ApiResponse<object>> DistinctAsync(
        string fieldName,
        Dictionary<string, object?>? filter = null)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (fieldName.Length == 0)
        {
            throw new ArgumentException("Field name must not be empty.", nameof(fieldName));
        }

        var encodedField = Uri.EscapeDataString(fieldName);

        return _client.RequestAsync<object>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/classes/{_encodedName}/distinct/{encodedField}",
            filter);
    }

    // ──────────────────────────────────────────────
    // Schema Management
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a new collection (schema) in the database.
    /// </summary>
    /// <returns>API response.</returns>
    public Task<Types.ApiResponse<object>> CreateAsync()
    {
        return _client.RequestAsync<object>(
            Types.HttpMethod.POST,
            $"/{Constants.ApiVersion}/schemas/{_encodedName}");
    }

    /// <summary>
    /// Clears all documents from the collection while keeping the schema.
    /// </summary>
    /// <returns>API response with <c>deletedCount</c> in the data.</returns>
    public Task<Types.ApiResponse<object>> ClearAsync()
    {
        return _client.RequestAsync<object>(
            Types.HttpMethod.PUT,
            $"/{Constants.ApiVersion}/schemas/{_encodedName}");
    }

    /// <summary>
    /// Drops the collection (schema and all documents) from the database.
    /// </summary>
    /// <returns>API response.</returns>
    public Task<Types.ApiResponse<object>> DropAsync()
    {
        return _client.RequestAsync<object>(
            Types.HttpMethod.DELETE,
            $"/{Constants.ApiVersion}/schemas/{_encodedName}");
    }
}

}
