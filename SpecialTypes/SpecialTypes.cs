using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace T1YOS.Sdk.SpecialTypes
{

/// <summary>
/// Special type marker functions and constants for the t1yOS SDK.
///
/// The t1yOS server recognizes special marker strings in JSON request bodies
/// and converts them to native database types (ObjectID, Date, typed numbers, etc.).
///
/// Examples:
/// <code>
/// SpecialTypes.ObjectID("507f1f77bcf86cd799439011")  → ObjectID('507f1f77bcf86cd799439011')
/// SpecialTypes.Date_("2024-01-15T10:30:00Z")         → Date('2024-01-15T10:30:00Z')
/// SpecialTypes.Integer(42)                            → Integer(42)
/// </code>
/// </summary>
public static class SpecialTypes
{
    /// <summary>ObjectID hex character pattern.</summary>
    private static readonly Regex ObjectIdPattern =
        new Regex(@"^[0-9a-fA-F]{24}$", RegexOptions.Compiled);

    // ──────────────────────────────────────────────
    // ObjectID
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates an ObjectID marker string for the given hex ID.
    /// The ID must be a 24-character hexadecimal string.
    /// </summary>
    /// <param name="id">24-character hexadecimal ObjectID.</param>
    /// <returns>An <c>ObjectID('...')</c> marker string.</returns>
    /// <exception cref="Utils.ValidationError">Thrown if the ID is not a valid 24-char hex string.</exception>
    public static string ObjectID(string id)
    {
        if (string.IsNullOrEmpty(id) || id.Length != 24)
        {
            throw new Utils.ValidationError("ObjectID must be exactly 24 characters.");
        }

        if (!ObjectIdPattern.IsMatch(id))
        {
            throw new Utils.ValidationError("ObjectID must be a valid hex string.");
        }

        return $"ObjectID('{id}')";
    }

    // ──────────────────────────────────────────────
    // Date types
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a Date marker string.
    /// </summary>
    /// <param name="dateStr">An ISO-8601 date string or other date representation.</param>
    /// <returns>A <c>Date('...')</c> marker string.</returns>
    public static string Date_(string dateStr) => $"Date('{dateStr}')";

    /// <summary>
    /// Creates a DateTime marker string.
    /// </summary>
    /// <param name="dateStr">An ISO-8601 datetime string.</param>
    /// <returns>A <c>DateTime('...')</c> marker string.</returns>
    public static string DateTime_(string dateStr) => $"DateTime('{dateStr}')";

    /// <summary>
    /// Creates a Timestamp marker string (Unix timestamp in seconds).
    /// </summary>
    /// <param name="unix">Unix timestamp as a string.</param>
    /// <returns>A <c>Timestamp('...')</c> marker string.</returns>
    public static string Timestamp(string unix) => $"Timestamp('{unix}')";

    // ──────────────────────────────────────────────
    // Numeric types
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a Boolean marker string.
    /// </summary>
    /// <param name="val">The boolean value.</param>
    /// <returns>A <c>Boolean(true)</c> or <c>Boolean(false)</c> marker string (lowercase).</returns>
    public static string Boolean_(bool val) => $"Boolean({val.ToString().ToLowerInvariant()})";

    /// <summary>
    /// Creates an Integer marker string.
    /// </summary>
    /// <param name="n">The integer value.</param>
    /// <returns>An <c>Integer(...)</c> marker string.</returns>
    public static string Integer(long n) => $"Integer({n})";

    /// <summary>
    /// Creates a Bigint marker string.
    /// </summary>
    /// <param name="n">The big integer value.</param>
    /// <returns>A <c>Bigint(...)</c> marker string.</returns>
    public static string Bigint(long n) => $"Bigint({n})";

    /// <summary>
    /// Creates a Float marker string.
    /// </summary>
    /// <param name="n">The float value.</param>
    /// <returns>A <c>Float(...)</c> marker string.</returns>
    public static string Float(double n) => $"Float({n.ToString(System.Globalization.CultureInfo.InvariantCulture)})";

    /// <summary>
    /// Creates a Double marker string.
    /// </summary>
    /// <param name="n">The double value.</param>
    /// <returns>A <c>Double(...)</c> marker string.</returns>
    public static string Double_(double n) => $"Double({n.ToString(System.Globalization.CultureInfo.InvariantCulture)})";

    // ──────────────────────────────────────────────
    // Structured types
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates an Array marker string. The array elements are serialized as JSON.
    /// </summary>
    /// <param name="arr">The array of objects.</param>
    /// <returns>An <c>Array(...)</c> marker string with JSON body.</returns>
    public static string Array_(object[] arr) => $"Array({JsonSerializer.Serialize(arr)})";

    /// <summary>
    /// Creates a Map marker string. The dictionary is serialized as JSON.
    /// </summary>
    /// <param name="obj">The dictionary to mark as a Map.</param>
    /// <returns>A <c>Map(...)</c> marker string with JSON body.</returns>
    public static string Map_(Dictionary<string, object?> obj) =>
        $"Map({JsonSerializer.Serialize(obj)})";

    /// <summary>
    /// Creates a MapArray marker string. The array of dictionaries is serialized as JSON.
    /// </summary>
    /// <param name="arr">Array of dictionaries to mark as Map[].</param>
    /// <returns>A <c>Map[](...)</c> marker string with JSON body.</returns>
    public static string MapArray(Dictionary<string, object?>[] arr) =>
        $"Map[]({JsonSerializer.Serialize(arr)})";

    // ──────────────────────────────────────────────
    // Null constants
    // ──────────────────────────────────────────────

    /// <summary>Null marker constant.</summary>
    public const string Null = "Null";

    /// <summary>None marker constant.</summary>
    public const string None = "None";

    /// <summary>Nil marker constant.</summary>
    public const string Nil = "Nil";

    /// <summary>Empty string marker constant.</summary>
    public const string Empty = "";

    /// <summary>UNDEFINED marker constant.</summary>
    public const string UNDEFINED = "UNDEFINED";

    /// <summary>Undefined marker constant.</summary>
    public const string Undefined = "Undefined";

    // ──────────────────────────────────────────────
    // Server-side time helpers
    // ──────────────────────────────────────────────

    /// <summary>Server-side <c>time.Now()</c> marker.</summary>
    public const string TimeNow = "time.Now()";

    /// <summary>Server-side <c>time.Now().Unix()</c> marker.</summary>
    public const string TimeNowUnix = "time.Now().Unix()";

    /// <summary>Server-side <c>time.Now().UnixNano()</c> marker.</summary>
    public const string TimeNowUnixNano = "time.Now().UnixNano()";

    /// <summary>Server-side <c>time.Now().Weekday()</c> marker.</summary>
    public const string TimeNowWeekday = "time.Now().Weekday()";

    /// <summary>Server-side <c>time.Now().Weekday().Chinese()</c> marker.</summary>
    public const string TimeNowWeekdayChinese = "time.Now().Weekday().Chinese()";

    // ──────────────────────────────────────────────
    // TimeNow convenience class
    // ──────────────────────────────────────────────

    /// <summary>
    /// Convenience class for server-side time markers.
    /// Provides a fluent-style API: <c>TimeNowHelper.Now()</c>, etc.
    /// </summary>
    public static class TimeNowHelper
    {
        /// <summary><c>time.Now()</c></summary>
        public static string Now() => TimeNow;

        /// <summary><c>time.Now().Unix()</c></summary>
        public static string Unix() => TimeNowUnix;

        /// <summary><c>time.Now().UnixNano()</c></summary>
        public static string UnixNano() => TimeNowUnixNano;

        /// <summary><c>time.Now().Weekday()</c></summary>
        public static string Weekday() => TimeNowWeekday;

        /// <summary><c>time.Now().Weekday().Chinese()</c></summary>
        public static string WeekdayChinese() => TimeNowWeekdayChinese;
    }
}

}
