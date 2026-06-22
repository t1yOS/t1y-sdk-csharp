# T1YOS.Sdk

**t1yOS Serverless 平台** 的 C# SDK — 云数据库、元数据和云函数客户端。

[![NuGet](https://img.shields.io/nuget/v/T1YOS.Sdk.svg)](https://www.nuget.org/packages/T1YOS.Sdk)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-4.8%20%7C%208%2B-blue)]()

## 特性

- **安全认证** — HMAC-SHA256 请求签名，带时间窗口验证
- **云数据库** — 完整的 CRUD 操作，支持 MongoDB 风格的查询（单文档、批量、分页、聚合）
- **Schema 管理** — 通过 API 创建、清空和删除集合
- **云函数** — 调用服务端 `.jsc` 函数，自动解析扩展名
- **安全模式** — AES-256-GCM 负载加密，端到端安全保障
- **特殊类型** — ObjectID、Date、Timestamp、类型化数值等标记字符串
- **时间同步** — 通过服务端时间同步自动校正时钟偏差
- **元数据** — 获取服务端元数据（版本、状态、集合列表）
- **多目标框架** — 支持 .NET Framework 4.8、.NET Core 2.0+、.NET 5–8+、Unity 和 Xamarin

## 支持的平台

| 目标框架         | 平台                                          |
| ---------------- | --------------------------------------------- |
| `netstandard2.0` | .NET Core 2.0+、.NET 5+、Mono、Xamarin、Unity |
| `net48`          | .NET Framework 4.8                            |
| `net8.0`         | .NET 8+（最佳性能）                           |

## 安装

### NuGet 包管理器

```bash
dotnet add package T1YOS.Sdk
```

### 包管理器控制台

```powershell
Install-Package T1YOS.Sdk
```

## 快速开始

```csharp
using T1YOS.Sdk;
using T1YOS.Sdk.Client;
using T1YOS.Sdk.SpecialTypes;

// 1. 创建客户端
var client = new T1YOS(new T1YOSConfig
{
    AppId = 1001,                                    // 您的应用 ID
    ApiKey = "your-32-character-api-key-here",       // 您的 API 密钥
    SecretKey = "your-32-character-secret-key-here", // 您的密钥
    BaseUrl = "https://myapp.t1y.net"                // 可选（默认值）
});

// 2. 初始化（同步服务器时间和安全模式）
await client.InitAsync();

// 3. 开始使用数据库
var users = client.Db.Collection("users");

// 插入文档
var insertResult = await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "name", "张三" },
    { "age", 30 },
    { "email", "zhangsan@example.com" },
    { "createdAt", SpecialTypes.TimeNow }  // 服务端时间戳
});
Console.WriteLine($"已插入: {insertResult.Data?.ObjectId}");

// 通过 ID 查找
var findResult = await users.FindByIdAsync(insertResult.Data!.ObjectId!);
Console.WriteLine($"已找到: {findResult.Data?.Result?["name"]}");
```

## 配置

| 选项         | 类型      | 必填 | 默认值                  | 说明                                                  |
| ------------ | --------- | ---- | ----------------------- | ----------------------------------------------------- |
| `AppId`      | `int`     | ✅   | —                       | 应用 ID（必须 ≥ 1001）                                |
| `ApiKey`     | `string`  | ✅   | —                       | API 密钥（必须恰好 32 个字符）                        |
| `SecretKey`  | `string`  | ✅   | —                       | 请求签名密钥（必须恰好 32 个字符）                    |
| `BaseUrl`    | `string?` | ❌   | `https://myapp.t1y.net` | 服务器基础 URL（必须以 `http://` 或 `https://` 开头） |
| `Version`    | `int?`    | ❌   | `0`                     | API 版本                                              |
| `IsSafeMode` | `bool?`   | ❌   | `false`                 | 启用 AES-256-GCM 负载加密                             |
| `TimeFormat` | `string?` | ❌   | `YYYY-MM-DD HH:mm:ss`   | 本地时间戳显示格式                                    |
| `Offset`     | `int?`    | ❌   | `0`                     | 时间偏移量（秒，由 `InitAsync()` 自动设置）           |

## 数据库操作

### 单文档操作

```csharp
// 插入
var result = await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "name", "李四" },
    { "score", 95 }
});

// 通过 ObjectID 查找
var doc = await users.FindByIdAsync("507f1f77bcf86cd799439011");

// 通过 ObjectID 更新
await users.UpdateByIdAsync("507f1f77bcf86cd799439011", new Dictionary<string, object?>
{
    { "$set", new Dictionary<string, object?> { { "score", 100 } } }
});

// 通过 ObjectID 删除
await users.DeleteByIdAsync("507f1f77bcf86cd799439011");
```

### 条件查询操作

```csharp
// 条件查找
var doc = await users.FindOneAsync(new Dictionary<string, object?>
{
    { "name", "张三" }
});

// 条件更新
await users.UpdateOneAsync(
    new Dictionary<string, object?> { { "name", "李四" } },
    new Dictionary<string, object?>
    {
        { "$inc", new Dictionary<string, object?> { { "score", 5 } } }
    });

// 条件删除
await users.DeleteOneAsync(new Dictionary<string, object?>
{
    { "status", "inactive" }
});
```

### 批量操作

```csharp
// 批量插入
var result = await users.InsertManyAsync(new[]
{
    new Dictionary<string, object?> { { "name", "张三" } },
    new Dictionary<string, object?> { { "name", "李四" } },
    new Dictionary<string, object?> { { "name", "王五" } }
});
Console.WriteLine($"已插入 {result.Data?.InsertedCount} 条文档");

// 批量更新
await users.UpdateManyAsync(
    new Dictionary<string, object?> { { "status", "pending" } },
    new Dictionary<string, object?>
    {
        { "$set", new Dictionary<string, object?> { { "status", "active" } } }
    });

// 批量删除
await users.DeleteManyAsync(new Dictionary<string, object?>
{
    { "archived", true }
});
```

### 高级查询

```csharp
// 分页查询
var page = await users.FindAsync(
    page: 1,
    size: 20,
    sort: new Dictionary<string, object?> { { "createdAt", -1 } },
    filter: new Dictionary<string, object?>
    {
        { "age", new Dictionary<string, object?> { { "$gte", 18 } } }
    });
Console.WriteLine($"第 {page.Data?.Page} 页，共 {page.Data?.Pagination?.TotalPages} 页");

// 聚合管道
var aggregateResult = await users.AggregateAsync(new[]
{
    new Dictionary<string, object?> { { "$match", new Dictionary<string, object?> { { "score", new Dictionary<string, object?> { { "$gt", 60 } } } } } },
    new Dictionary<string, object?> { { "$group", new Dictionary<string, object?> { { "_id", "$name" }, { "total", new Dictionary<string, object?> { { "$sum", "$score" } } } } } }
});

// 统计文档数
var countResult = await users.CountAsync(new Dictionary<string, object?>
{
    { "status", "active" }
});

// 去重值
var distinctResult = await users.DistinctAsync("category");
```

### Schema 管理

```csharp
// 创建集合
await users.CreateAsync();

// 清空集合（保留 Schema）
await users.ClearAsync();

// 删除集合（同时删除 Schema 和所有文档）
await users.DropAsync();
```

## 特殊类型

t1yOS 服务端能识别 JSON 请求体中的特殊标记字符串。使用 `SpecialTypes` 类来创建它们：

| 方法 / 常量                                      | 输出                               | 说明                  |
| ------------------------------------------------ | ---------------------------------- | --------------------- |
| `SpecialTypes.ObjectID("507f...")`               | `ObjectID('507f...')`              | MongoDB ObjectID 标记 |
| `SpecialTypes.Date_("2024-01-15T10:30:00Z")`     | `Date('2024-01-15T10:30:00Z')`     | 日期标记              |
| `SpecialTypes.DateTime_("2024-01-15T10:30:00Z")` | `DateTime('2024-01-15T10:30:00Z')` | 日期时间标记          |
| `SpecialTypes.Timestamp("1705312200")`           | `Timestamp('1705312200')`          | Unix 时间戳标记       |
| `SpecialTypes.Boolean_(true)`                    | `Boolean(true)`                    | 布尔值标记            |
| `SpecialTypes.Integer(42)`                       | `Integer(42)`                      | 整型标记              |
| `SpecialTypes.Bigint(9007199254740991)`          | `Bigint(9007199254740991)`         | 大整数标记            |
| `SpecialTypes.Float(3.14)`                       | `Float(3.14)`                      | 浮点数标记            |
| `SpecialTypes.Double_(3.14159265)`               | `Double(3.14159265)`               | 双精度浮点数标记      |
| `SpecialTypes.Array_(new object[] {1,2,3})`      | `Array([1,2,3])`                   | 数组标记              |
| `SpecialTypes.Map_(dict)`                        | `Map({"key":"value"})`             | 映射标记              |
| `SpecialTypes.MapArray(dicts)`                   | `Map[]([...])`                     | 映射数组标记          |
| `SpecialTypes.Null`                              | `Null`                             | Null 标记常量         |
| `SpecialTypes.None`                              | `None`                             | None 标记常量         |
| `SpecialTypes.Nil`                               | `Nil`                              | Nil 标记常量          |
| `SpecialTypes.Empty`                             | `""`                               | 空字符串标记          |
| `SpecialTypes.UNDEFINED`                         | `UNDEFINED`                        | Undefined 标记        |
| `SpecialTypes.TimeNow`                           | `time.Now()`                       | 服务端当前时间        |
| `SpecialTypes.TimeNowUnix`                       | `time.Now().Unix()`                | 服务端 Unix 时间戳    |
| `SpecialTypes.TimeNowUnixNano`                   | `time.Now().UnixNano()`            | 服务端纳秒时间戳      |
| `SpecialTypes.TimeNowWeekday`                    | `time.Now().Weekday()`             | 服务端星期几（数字）  |
| `SpecialTypes.TimeNowWeekdayChinese`             | `time.Now().Weekday().Chinese()`   | 服务端星期几（中文）  |

**注意：** 以 `_` 结尾的方法（`Date_`、`Boolean_`、`Double_`、`Array_`、`Map_`、`DateTime_`）添加了下划线后缀，以避免与 C# 内置类型冲突。

### 自动日期转换

SDK 会自动将请求体中的 C# `DateTime` 对象转换为 `Date('...')` 标记，将 10 位及以上的数字转换为 `Timestamp('...')` 标记。例如：

```csharp
await users.InsertOneAsync(new Dictionary<string, object?>
{
    { "scheduledAt", DateTime.UtcNow },       // → Date('2024-01-15T...')
    { "version", 1705312200L }                 // → Timestamp('1705312200')
});
```

## 云函数

调用服务端 JavaScript（`.jsc`）函数：

```csharp
// 使用参数调用 "hello.jsc"
var result = await client.CallFuncAsync("hello", new
{
    name = "World",
    greeting = "你好"
});

// 扩展名规则：
// "hello"     → "hello.jsc"
// "dir/"      → "dir/index.jsc"
// "script.js" → "script.jsc"
// "func.jsc"  → "func.jsc"（无变化）
```

## 元数据

```csharp
// 获取所有元数据
var meta = await client.GetMetaAsync();

// 获取特定字段
var versionInfo = await client.GetMetaAsync("version");
```

## 安全性

### 请求签名

每个 API 请求都使用 HMAC-SHA256 进行签名：

```
签名    = HMAC-SHA256(secretKey, message)
message = METHOD + "\n" + PATH_AND_QUERY + "\n" + SHA256(body) + "\n" + appId + "\n" + timestamp
```

签名通过 `X-T1Y-Safe-Sign` 请求头发送，同时发送的还有：

- `X-T1Y-Application-ID` — 您的应用 ID
- `X-T1Y-API-Key` — 您的 API 密钥
- `X-T1Y-Safe-Timestamp` — Unix 时间戳（已根据服务端偏移量调整）

### 安全模式（AES-256-GCM 加密）

当启用安全模式时（通过 `IsSafeMode: true` 或在 `InitAsync()` 期间自动检测），请求和响应体将使用 AES-256-GCM 进行加密：

- **密钥**：您的 32 字符密钥（UTF-8 编码 → 32 字节）
- **Nonce**：12 个随机字节（每条消息独立）
- **Tag**：128 位认证标签
- **负载格式**：`{"n": "<base64 nonce>", "j": "<base64 ciphertext>", "t": "<base64 tag>"}`

这确保了敏感数据的端到端机密性和完整性。

## 错误处理

```csharp
try
{
    var result = await users.InsertOneAsync(data);
}
catch (T1YError ex)
{
    Console.WriteLine($"API 错误 [{ex.Code}]: {ex.Message}");
    // ex.ErrorData 可能包含额外信息
}
catch (ValidationError ex)
{
    Console.WriteLine($"验证错误: {ex.Message}");
}
```

## API 参考

### T1YOS 客户端

| 方法                                                | 说明                         |
| --------------------------------------------------- | ---------------------------- |
| `InitAsync()`                                       | 同步服务器时间和安全模式设置 |
| `RequestAsync<T>(method, path, body?, encryption?)` | 核心请求方法                 |
| `GetMetaAsync(field?)`                              | 获取服务端元数据             |
| `CallFuncAsync(name, parameters?, enableSafeMode?)` | 调用云函数                   |

### T1Collection

| 方法                                    | 说明                   |
| --------------------------------------- | ---------------------- |
| `InsertOneAsync(data)`                  | 插入单条文档           |
| `FindByIdAsync(objectId)`               | 通过 ObjectID 查找文档 |
| `UpdateByIdAsync(objectId, data)`       | 通过 ObjectID 更新文档 |
| `DeleteByIdAsync(objectId)`             | 通过 ObjectID 删除文档 |
| `FindOneAsync(filter)`                  | 通过条件查找一条文档   |
| `UpdateOneAsync(filter, body)`          | 通过条件更新一条文档   |
| `DeleteOneAsync(filter)`                | 通过条件删除一条文档   |
| `InsertManyAsync(dataList)`             | 批量插入文档           |
| `UpdateManyAsync(filter, body)`         | 批量更新文档           |
| `DeleteManyAsync(filter)`               | 批量删除文档           |
| `FindAsync(page, size, sort?, filter?)` | 分页查询               |
| `AggregateAsync(pipeline)`              | 执行聚合管道           |
| `CountAsync(filter?)`                   | 统计文档数量           |
| `DistinctAsync(fieldName, filter?)`     | 获取字段去重值         |
| `CreateAsync()`                         | 创建集合 Schema        |
| `ClearAsync()`                          | 清空所有文档           |
| `DropAsync()`                           | 删除整个集合           |

## 开源协议

本项目基于 [MIT License](LICENSE) 开源。

Copyright (c) 2026 华易云联（杭州）网络科技有限责任公司
