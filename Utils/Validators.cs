using System;
using System.Text.RegularExpressions;

namespace T1YOS.Sdk.Utils
{

/// <summary>
/// Validation utilities for the T1YOS SDK configuration and data.
/// </summary>
public static class Validators
{
    /// <summary>
    /// Validates that the application ID is >= 1001.
    /// </summary>
    /// <param name="appId">Application ID to validate.</param>
    /// <exception cref="ValidationError">Thrown if appId is less than 1001.</exception>
    public static void ValidateAppId(int appId)
    {
        if (appId < Constants.MinAppId)
        {
            throw new ValidationError(
                $"appId must be an integer >= {Constants.MinAppId}.");
        }
    }

    /// <summary>
    /// Validates that the API key is exactly 32 characters.
    /// </summary>
    /// <param name="apiKey">API key string to validate.</param>
    /// <exception cref="ValidationError">Thrown if apiKey is not 32 characters.</exception>
    public static void ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length != Constants.ApiKeyLength)
        {
            throw new ValidationError(
                $"apiKey must be exactly {Constants.ApiKeyLength} characters.");
        }
    }

    /// <summary>
    /// Validates that the secret key is exactly 32 characters.
    /// </summary>
    /// <param name="secretKey">Secret key string to validate.</param>
    /// <exception cref="ValidationError">Thrown if secretKey is not 32 characters.</exception>
    public static void ValidateSecretKey(string secretKey)
    {
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length != Constants.SecretKeyLength)
        {
            throw new ValidationError(
                $"secretKey must be exactly {Constants.SecretKeyLength} characters.");
        }
    }

    /// <summary>
    /// Validates that the base URL starts with http:// or https://.
    /// </summary>
    /// <param name="baseUrl">Base URL string to validate.</param>
    /// <exception cref="ValidationError">Thrown if baseUrl is missing required protocol.</exception>
    public static void ValidateBaseUrl(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl) ||
            (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
             !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationError(
                "baseUrl must start with http:// or https://.");
        }
    }

    /// <summary>
    /// Validates all required fields in a T1YOSConfig object.
    /// </summary>
    /// <param name="config">Configuration to validate.</param>
    /// <exception cref="ValidationError">Thrown if any required field is invalid.</exception>
    public static void ValidateInitConfig(Types.T1YOSConfig config)
    {
        if (config == null)
        {
            throw new ValidationError("config is required.");
        }

        ValidateAppId(config.AppId);

        if (config.ApiKey == null)
        {
            throw new ValidationError("apiKey is required.");
        }
        ValidateApiKey(config.ApiKey);

        if (config.SecretKey == null)
        {
            throw new ValidationError("secretKey is required.");
        }
        ValidateSecretKey(config.SecretKey);

        if (config.BaseUrl != null)
        {
            ValidateBaseUrl(config.BaseUrl);
        }

        if (config.Version.HasValue && config.Version.Value < 0)
        {
            throw new ValidationError("version must be a non-negative integer.");
        }
    }

    /// <summary>
    /// Validates that a string is a valid 24-character hexadecimal ObjectID.
    /// </summary>
    /// <param name="idStr">The ObjectID string to validate.</param>
    /// <param name="name">Optional name for the error message.</param>
    /// <returns>True if the ObjectID is valid.</returns>
    /// <exception cref="ValidationError">Thrown if the string is not a valid ObjectID.</exception>
    public static bool AssertObjectID(string idStr, string name = "ObjectID")
    {
        if (string.IsNullOrEmpty(idStr) || idStr.Length != Constants.ObjectIdLength)
        {
            throw new ValidationError(
                $"{name} must be exactly {Constants.ObjectIdLength} characters.");
        }

        if (!Regex.IsMatch(idStr, @"^[0-9a-fA-F]{" + Constants.ObjectIdLength + "}$"))
        {
            throw new ValidationError($"{name} must be a valid hex string.");
        }

        return true;
    }
}

}
