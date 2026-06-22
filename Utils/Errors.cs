using System;

namespace T1YOS.Sdk.Utils
{

/// <summary>
/// Represents an error returned by the t1yOS API.
/// </summary>
public class T1YError : Exception
{
    /// <summary>
    /// HTTP status code or error code from the server.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Optional response data payload associated with the error.
    /// </summary>
    public object? ErrorData { get; }

    /// <summary>
    /// Creates a new T1YError instance.
    /// </summary>
    /// <param name="code">Error code (HTTP status or custom).</param>
    /// <param name="message">Error message.</param>
    /// <param name="data">Optional error data.</param>
    public T1YError(int code, string message, object? data = null)
        : base(message)
    {
        Code = code;
        ErrorData = data;
    }

    /// <summary>
    /// Returns a JSON-like string representation of the error.
    /// </summary>
    public override string ToString()
    {
        return $"[T1YError] Code: {Code}, Message: {Message}";
    }
}

/// <summary>
/// Represents a configuration validation error.
/// </summary>
public class ValidationError : Exception
{
    /// <summary>
    /// Creates a new ValidationError instance.
    /// </summary>
    /// <param name="message">Validation error message.</param>
    public ValidationError(string message) : base(message)
    {
    }
}
}

