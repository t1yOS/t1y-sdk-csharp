using System;
using System.Collections.Generic;
using System.Linq;

namespace T1YOS.Sdk.Utils
{

/// <summary>
/// Type conversion utilities for the T1YOS SDK.
/// Handles conversion of C# DateTime objects and numeric timestamps
/// into the special marker strings that the t1yOS server understands.
/// </summary>
public static class ConvertHelper
{
    /// <summary>
    /// Recursively converts DateTime objects and timestamp numbers
    /// into special marker strings recognized by the t1yOS server.
    ///
    /// Rules:
    /// - <see cref="DateTime"/> objects become <c>Date('ISO-8601')</c> marker strings.
    /// - <see cref="long"/> values with 10+ digits become <c>Timestamp('unix')</c> markers.
    /// - <see cref="Dictionary{TKey, TValue}"/> objects are traversed recursively.
    /// - Arrays and lists are traversed recursively.
    /// - All other values are returned unchanged.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value with Date/Timestamp markers applied.</returns>
    public static object? ConvertDateTypes(object? value)
    {
        if (value == null)
        {
            return null;
        }

        // DateTime → Date('ISO-8601')
        if (value is DateTime dt)
        {
            return $"Date('{dt.ToString("O")}')";
        }

        // DateTimeOffset → Date('ISO-8601')
        if (value is DateTimeOffset dto)
        {
            return $"Date('{dto.ToString("O")}')";
        }

        // long >= 10 digits → Timestamp('unix')
        if (value is long longVal && longVal >= 1000000000L)
        {
            return $"Timestamp('{longVal}')";
        }

        // int >= 10 digits → Timestamp('unix')
        if (value is int intVal && intVal >= 1000000000)
        {
            return $"Timestamp('{intVal}')";
        }

        // double >= 10 digits (no fractional part) → Timestamp('unix')
        if (value is double doubleVal && doubleVal >= 1000000000.0 && doubleVal == Math.Floor(doubleVal))
        {
            return $"Timestamp('{(long)doubleVal}')";
        }

        // float >= 10 digits (no fractional part) → Timestamp('unix')
        if (value is float floatVal && floatVal >= 1000000000.0f && floatVal == (float)Math.Floor(floatVal))
        {
            return $"Timestamp('{(long)floatVal}')";
        }

        // Recursively process dictionaries
        if (value is Dictionary<string, object?> dict)
        {
            var result = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                result[kvp.Key] = ConvertDateTypes(kvp.Value);
            }
            return result;
        }

        // Recursively process lists/arrays
        if (value is System.Collections.IList list && !(value is string))
        {
            var result = new object?[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = ConvertDateTypes(list[i]);
            }
            return result;
        }

        // Return unchanged
        return value;
    }

    /// <summary>
    /// Checks if a value is a non-null, non-array dictionary with at least one key.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a non-empty object (dictionary).</returns>
    public static bool IsNonEmptyObject(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is Dictionary<string, object?> dict)
        {
            return dict.Count > 0;
        }

        return false;
    }

    /// <summary>
    /// Checks if a value is a non-null dictionary (may be empty).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a plain object (dictionary).</returns>
    public static bool IsPlainObject(object? value)
    {
        if (value == null)
        {
            return false;
        }

        return value is Dictionary<string, object?>;
    }

    /// <summary>
    /// Checks if a value is a non-empty array where every element is a non-empty dictionary.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a non-empty array of non-empty objects.</returns>
    public static bool IsNonEmptyArrayWithNonEmptyObjects(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is Dictionary<string, object?>[] arr)
        {
            if (arr.Length == 0)
            {
                return false;
            }

            return arr.All(item => item != null && item.Count > 0);
        }

        if (value is object?[] objArr)
        {
            if (objArr.Length == 0)
            {
                return false;
            }

            return objArr.All(item =>
                item is Dictionary<string, object?> d && d.Count > 0);
        }

        return false;
    }
}

}
