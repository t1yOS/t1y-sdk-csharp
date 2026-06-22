using System;
using System.Collections.Generic;
using System.Globalization;

namespace T1YOS.Sdk.Utils
{

/// <summary>
/// Time formatting utilities for the T1YOS SDK.
/// Handles conversion of server UTC timestamps to local time format.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// Recursively formats <c>createdAt</c> and <c>updatedAt</c> fields
    /// from UTC strings to local time using the specified format.
    ///
    /// Format tokens:
    /// <c>YYYY</c> = year, <c>MM</c> = month, <c>DD</c> = day,
    /// <c>HH</c> = hour, <c>mm</c> = minute, <c>ss</c> = second.
    /// </summary>
    /// <param name="data">The data structure to format.</param>
    /// <param name="format">Custom time format string. Defaults to <c>"YYYY-MM-DD HH:mm:ss"</c>.</param>
    /// <returns>The data structure with timestamps formatted to local time.</returns>
    public static object? FormatTimestampsToLocal(
        object? data,
        string format = Constants.DefaultTimeFormat)
    {
        if (data == null)
        {
            return null;
        }

        // Dictionary case: check for createdAt/updatedAt keys
        if (data is Dictionary<string, object?> dict)
        {
            var result = new Dictionary<string, object?>();
            foreach (var kvp in dict)
            {
                if ((kvp.Key == "createdAt" || kvp.Key == "updatedAt") &&
                    kvp.Value is string dateStr)
                {
                    result[kvp.Key] = FormatDateString(dateStr, format);
                }
                else
                {
                    result[kvp.Key] = FormatTimestampsToLocal(kvp.Value, format);
                }
            }
            return result;
        }

        // Array case: recursively format each element
        if (data is System.Collections.IList list && !(data is string))
        {
            var result = new object?[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = FormatTimestampsToLocal(list[i], format);
            }
            return result;
        }

        return data;
    }

    /// <summary>
    /// Formats a UTC date string to local time using the specified format pattern.
    /// </summary>
    /// <param name="utcDateString">UTC date string to parse and format.</param>
    /// <param name="format">Custom format with YYYY, MM, DD, HH, mm, ss tokens.</param>
    /// <returns>Local time formatted string, or the original string if parsing fails.</returns>
    private static string FormatDateString(string utcDateString, string format)
    {
        if (!DateTime.TryParse(utcDateString, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var utcDateTime))
        {
            return utcDateString;
        }

        // Convert to local time
        var localTime = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime.ToLocalTime()
            : utcDateTime;

        // Apply custom format tokens
        var result = format
            .Replace("YYYY", localTime.Year.ToString("D4"))
            .Replace("MM", localTime.Month.ToString("D2"))
            .Replace("DD", localTime.Day.ToString("D2"))
            .Replace("HH", localTime.Hour.ToString("D2"))
            .Replace("mm", localTime.Minute.ToString("D2"))
            .Replace("ss", localTime.Second.ToString("D2"));

        return result;
    }
}

}
