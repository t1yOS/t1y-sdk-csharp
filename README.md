# T1YOS.Sdk

C# SDK for the **t1yOS Serverless Platform** — cloud database, metadata, and cloud functions client.

[![NuGet](https://img.shields.io/nuget/v/T1YOS.Sdk.svg)](https://www.nuget.org/packages/T1YOS.Sdk)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-4.8%20%7C%208%2B-blue)]()

## Features

- **Secure Authentication** — HMAC-SHA256 request signing with time-window validation
- **Cloud Database** — Full CRUD operations with MongoDB-style queries (single, bulk, paginated, aggregation)
- **Schema Management** — Create, clear, and drop collections programmatically
- **Cloud Functions** — Invoke server-side `.jsc` functions with automatic extension resolution
- **Safe Mode** — AES-256-GCM payload encryption for end-to-end security
- **Special Types** — Marker strings for ObjectID, Date, Timestamp, typed numerics, and more
- **Time Synchronization** — Automatic clock-skew correction with server time sync
- **Metadata** — Retrieve server metadata (version, status, collections)
- **Multi-targeting** — Supports .NET Framework 4.8, .NET Core 2.0+, .NET 5–8+, Unity, and Xamarin

## Supported Platforms

| Target Framework | Platforms                                     |
| ---------------- | --------------------------------------------- |
| `netstandard2.0` | .NET Core 2.0+, .NET 5+, Mono, Xamarin, Unity |
| `net48`          | .NET Framework 4.8                            |
| `net8.0`         | .NET 8+ (best performance)                    |

## Installation

### NuGet Package Manager

```bash
dotnet add package T1YOS.Sdk
```

### Package Manager Console

```powershell
Install-Package T1YOS.Sdk
```

## Quick Start

```csharp
using T1YOS.Sdk;
using T1YOS.Sdk.Client;
using T1YOS.Sdk.SpecialTypes;

// 1. Create the client
var client = new T1YOS(new T1YOSConfig
{
    AppId = 1001,                                    // Your application ID
    ApiKey = "your-32-character-api-key-here",       // Your API key
    SecretKey = "your-32-character-secret-key-here", // Your secret key
    BaseUrl = "https://myapp.t1y.net"                // Optional (this is the default)
});

// 2. Initialize (syncs server time and safe mode)
await client.InitAsync();

// 3. Start using the database
var users = client.Db.Collection("users");

// Insert a document
var insertResult = await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "name", "Alice" },
    { "age", 30 },
    { "email", "alice@example.com" },
    { "createdAt", SpecialTypes.TimeNow }  // Server-side timestamp
});
Console.WriteLine($"Inserted: {insertResult.Data?.ObjectId}");

// Find by ID
var findResult = await users.FindByIdAsync(insertResult.Data!.ObjectId!);
Console.WriteLine($"Found: {findResult.Data?.Result?["name"]}");
```

## Configuration

| Option       | Type      | Required | Default                 | Description                                               |
| ------------ | --------- | -------- | ----------------------- | --------------------------------------------------------- |
| `AppId`      | `int`     | ✅       | —                       | Application ID (must be ≥ 1001)                           |
| `ApiKey`     | `string`  | ✅       | —                       | API key (exactly 32 characters)                           |
| `SecretKey`  | `string`  | ✅       | —                       | Secret key for request signing (exactly 32 characters)    |
| `BaseUrl`    | `string?` | ❌       | `https://myapp.t1y.net` | Server base URL (must start with `http://` or `https://`) |
| `Version`    | `int?`    | ❌       | `0`                     | API version                                               |
| `IsSafeMode` | `bool?`   | ❌       | `false`                 | Enable AES-256-GCM payload encryption                     |
| `TimeFormat` | `string?` | ❌       | `YYYY-MM-DD HH:mm:ss`   | Format for local timestamp display                        |
| `Offset`     | `int?`    | ❌       | `0`                     | Time offset in seconds (auto-set by `InitAsync()`)        |

## Database Operations

### Single Document Operations

```csharp
// Insert
var result = await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "name", "Bob" },
    { "score", 95 }
});

// Find by ObjectID
var doc = await users.FindByIdAsync("507f1f77bcf86cd799439011");

// Update by ObjectID
await users.UpdateByIdAsync("507f1f77bcf86cd799439011", new Dictionary<string, object?>
{
    { "$set", new Dictionary<string, object?> { { "score", 100 } } }
});

// Delete by ObjectID
await users.DeleteByIdAsync("507f1f77bcf86cd799439011");
```

### Filter-based Operations

```csharp
// Find one by filter
var doc = await users.FindOneAsync(new Dictionary<string, object?>
{
    { "name", "Alice" }
});

// Update one by filter
await users.UpdateOneAsync(
    new Dictionary<string, object?> { { "name", "Bob" } },
    new Dictionary<string, object?>
    {
        { "$inc", new Dictionary<string, object?> { { "score", 5 } } }
    });

// Delete one by filter
await users.DeleteOneAsync(new Dictionary<string, object?>
{
    { "status", "inactive" }
});
```

### Bulk Operations

```csharp
// Insert many
var result = await users.InsertManyAsync(new[]
{
    new Dictionary<string, object?> { { "name", "Alice" } },
    new Dictionary<string, object?> { { "name", "Bob" } },
    new Dictionary<string, object?> { { "name", "Charlie" } }
});
Console.WriteLine($"Inserted {result.Data?.InsertedCount} documents");

// Update many
await users.UpdateManyAsync(
    new Dictionary<string, object?> { { "status", "pending" } },
    new Dictionary<string, object?>
    {
        { "$set", new Dictionary<string, object?> { { "status", "active" } } }
    });

// Delete many
await users.DeleteManyAsync(new Dictionary<string, object?>
{
    { "archived", true }
});
```

### Advanced Queries

```csharp
// Paginated find
var page = await users.FindAsync(
    page: 1,
    size: 20,
    sort: new Dictionary<string, object?> { { "createdAt", -1 } },
    filter: new Dictionary<string, object?>
    {
        { "age", new Dictionary<string, object?> { { "$gte", 18 } } }
    });
Console.WriteLine($"Page {page.Data?.Page} of {page.Data?.Pagination?.TotalPages}");

// Aggregation pipeline
var aggregateResult = await users.AggregateAsync(new[]
{
    new Dictionary<string, object?> { { "$match", new Dictionary<string, object?> { { "score", new Dictionary<string, object?> { { "$gt", 60 } } } } } },
    new Dictionary<string, object?> { { "$group", new Dictionary<string, object?> { { "_id", "$name" }, { "total", new Dictionary<string, object?> { { "$sum", "$score" } } } } } }
});

// Count documents
var countResult = await users.CountAsync(new Dictionary<string, object?>
{
    { "status", "active" }
});

// Distinct values
var distinctResult = await users.DistinctAsync("category");
```

### Schema Management

```csharp
// Create collection
await users.CreateAsync();

// Clear all documents (keeps schema)
await users.ClearAsync();

// Drop collection (removes schema and documents)
await users.DropAsync();
```

## Special Types

The t1yOS server recognizes special marker strings in JSON request bodies. Use the `SpecialTypes` class to create them:

| Method / Constant                                | Output                             | Description                      |
| ------------------------------------------------ | ---------------------------------- | -------------------------------- |
| `SpecialTypes.ObjectID("507f...")`               | `ObjectID('507f...')`              | MongoDB ObjectID marker          |
| `SpecialTypes.Date_("2024-01-15T10:30:00Z")`     | `Date('2024-01-15T10:30:00Z')`     | Date marker                      |
| `SpecialTypes.DateTime_("2024-01-15T10:30:00Z")` | `DateTime('2024-01-15T10:30:00Z')` | DateTime marker                  |
| `SpecialTypes.Timestamp("1705312200")`           | `Timestamp('1705312200')`          | Unix timestamp marker            |
| `SpecialTypes.Boolean_(true)`                    | `Boolean(true)`                    | Boolean marker                   |
| `SpecialTypes.Integer(42)`                       | `Integer(42)`                      | Typed integer marker             |
| `SpecialTypes.Bigint(9007199254740991)`          | `Bigint(9007199254740991)`         | Typed big integer marker         |
| `SpecialTypes.Float(3.14)`                       | `Float(3.14)`                      | Typed float marker               |
| `SpecialTypes.Double_(3.14159265)`               | `Double(3.14159265)`               | Typed double marker              |
| `SpecialTypes.Array_(new object[] {1,2,3})`      | `Array([1,2,3])`                   | Typed array marker               |
| `SpecialTypes.Map_(dict)`                        | `Map({"key":"value"})`             | Typed map marker                 |
| `SpecialTypes.MapArray(dicts)`                   | `Map[]([...])`                     | Typed map-array marker           |
| `SpecialTypes.Null`                              | `Null`                             | Null marker constant             |
| `SpecialTypes.None`                              | `None`                             | None marker constant             |
| `SpecialTypes.Nil`                               | `Nil`                              | Nil marker constant              |
| `SpecialTypes.Empty`                             | `""`                               | Empty string marker              |
| `SpecialTypes.UNDEFINED`                         | `UNDEFINED`                        | Undefined marker                 |
| `SpecialTypes.TimeNow`                           | `time.Now()`                       | Server-side current time         |
| `SpecialTypes.TimeNowUnix`                       | `time.Now().Unix()`                | Server-side Unix timestamp       |
| `SpecialTypes.TimeNowUnixNano`                   | `time.Now().UnixNano()`            | Server-side nanosecond timestamp |
| `SpecialTypes.TimeNowWeekday`                    | `time.Now().Weekday()`             | Server-side weekday (number)     |
| `SpecialTypes.TimeNowWeekdayChinese`             | `time.Now().Weekday().Chinese()`   | Server-side weekday (Chinese)    |

**Note:** Methods ending with `_` (`Date_`, `Boolean_`, `Double_`, `Array_`, `Map_`, `DateTime_`) have the underscore suffix to avoid conflicts with C# built-in types.

### Automatic Date Conversion

The SDK automatically converts C# `DateTime` objects to `Date('...')` markers and 10+ digit numbers to `Timestamp('...')` markers in request bodies. For example:

```csharp
await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "scheduledAt", DateTime.UtcNow },       // → Date('2024-01-15T...')
    { "version", 1705312200L }                 // → Timestamp('1705312200')
});
```

## Cloud Functions

Call server-side JavaScript (`.jsc`) functions:

```csharp
// Call "hello.jsc" with parameters
var result = await client.CallFuncAsync("hello", new
{
    name = "World",
    greeting = "你好"
});

// Extension rules:
// "hello"     → "hello.jsc"
// "dir/"      → "dir/index.jsc"
// "script.js" → "script.jsc"
// "func.jsc"  → "func.jsc" (no change)
```

## Metadata

```csharp
// Get all metadata
var meta = await client.GetMetaAsync();

// Get specific field
var versionInfo = await client.GetMetaAsync("version");
```

## Security

### Request Signing

Every API request is signed using HMAC-SHA256:

```
signature = HMAC-SHA256(secretKey, message)
message   = METHOD + "\n" + PATH_AND_QUERY + "\n" + SHA256(body) + "\n" + appId + "\n" + timestamp
```

The signature is sent in the `X-T1Y-Safe-Sign` header along with:

- `X-T1Y-Application-ID` — Your application ID
- `X-T1Y-API-Key` — Your API key
- `X-T1Y-Safe-Timestamp` — Unix timestamp (adjusted by server offset)

### Safe Mode (AES-256-GCM Encryption)

When enabled (via `IsSafeMode: true` or auto-detected during `InitAsync()`), request and response bodies are encrypted with AES-256-GCM:

- **Key**: Your 32-character secret key (UTF-8 encoded → 32 bytes)
- **Nonce**: 12 random bytes (per-message)
- **Tag**: 128-bit authentication tag
- **Payload Format**: `{"n": "<base64 nonce>", "j": "<base64 ciphertext>", "t": "<base64 tag>"}`

This ensures end-to-end confidentiality and integrity for sensitive data.

## Error Handling

```csharp
try
{
    var result = await users.InsertOneAsync(data);
}
catch (T1YError ex)
{
    Console.WriteLine($"API Error [{ex.Code}]: {ex.Message}");
    // ex.ErrorData may contain additional details
}
catch (ValidationError ex)
{
    Console.WriteLine($"Validation Error: {ex.Message}");
}
```

## API Reference

### T1YOS Client

| Method                                              | Description                            |
| --------------------------------------------------- | -------------------------------------- |
| `InitAsync()`                                       | Sync server time and safe mode setting |
| `RequestAsync<T>(method, path, body?, encryption?)` | Core request method                    |
| `GetMetaAsync(field?)`                              | Retrieve server metadata               |
| `CallFuncAsync(name, parameters?, enableSafeMode?)` | Invoke cloud function                  |

### T1Collection

| Method                                  | Description                   |
| --------------------------------------- | ----------------------------- |
| `InsertOneAsync(data)`                  | Insert a single document      |
| `FindByIdAsync(objectId)`               | Find document by ObjectID     |
| `UpdateByIdAsync(objectId, data)`       | Update document by ObjectID   |
| `DeleteByIdAsync(objectId)`             | Delete document by ObjectID   |
| `FindOneAsync(filter)`                  | Find one document by filter   |
| `UpdateOneAsync(filter, body)`          | Update one document by filter |
| `DeleteOneAsync(filter)`                | Delete one document by filter |
| `InsertManyAsync(dataList)`             | Insert multiple documents     |
| `UpdateManyAsync(filter, body)`         | Update multiple documents     |
| `DeleteManyAsync(filter)`               | Delete multiple documents     |
| `FindAsync(page, size, sort?, filter?)` | Paginated find                |
| `AggregateAsync(pipeline)`              | Execute aggregation pipeline  |
| `CountAsync(filter?)`                   | Count documents               |
| `DistinctAsync(fieldName, filter?)`     | Get distinct field values     |
| `CreateAsync()`                         | Create collection schema      |
| `ClearAsync()`                          | Clear all documents           |
| `DropAsync()`                           | Drop collection               |

## License

This project is licensed under the [MIT License](LICENSE).

Copyright (c) 2026 华易云联（杭州）网络科技有限责任公司
