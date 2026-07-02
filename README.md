# ShodanClient

[![CI](https://github.com/AriusII/ShodanClient/actions/workflows/ci.yml/badge.svg)](https://github.com/AriusII/ShodanClient/actions/workflows/ci.yml)
[![Release](https://github.com/AriusII/ShodanClient/actions/workflows/release.yml/badge.svg)](https://github.com/AriusII/ShodanClient/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/ShodanClient.svg)](https://www.nuget.org/packages/ShodanClient)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A modern, fully-typed, high-performance .NET 10 / C# 14 client for the [Shodan API](https://developer.shodan.io/api)
covering all five API surfaces: **Search**, **On-Demand Scanning**, **Network Alerts**, **Notifiers**, **Directory**, *
*DNS**, **Account**, **Organization**, **Bulk Data**, **Utility**, **API Status**, **Streaming**, **Trends**, **Exploits
** and **InternetDB**.

> **Status:** feature-complete against Shodan's published REST, Streaming, Trends and InternetDB APIs.
> Native-AOT/trim-safe, source-generated JSON throughout.

## Why

Shodan exposes its functionality across five distinct API surfaces, each with its own base URL and authentication style:
the core REST API, the long-lived Streaming API, the historical Trends API, the key-less InternetDB, and the Exploits
API. `ShodanClient` wraps all of them behind a single, ergonomic, dependency-injection-friendly client with first-class
support for resilience, client-side rate limiting, cancellation, and Native AOT.

```csharp
builder.Services.AddShodanClient(builder.Configuration); // binds the "Shodan" config section

var shodan = app.Services.GetRequiredService<IShodanClient>();

var host = await shodan.Hosts.GetAsync("8.8.8.8");
var results = await shodan.Search.SearchAsync("apache country:US");
var summary = await shodan.InternetDb.GetAsync("1.1.1.1"); // no API key required

await foreach (var banner in shodan.Stream.StreamAllBannersAsync())
{
    Console.WriteLine($"{banner.IpString}:{banner.Port}");
}
```

## Architecture

The solution follows Clean Architecture, with all five Shodan API surfaces modeled as bounded contexts inside the same
layers rather than as separate packages:

```
ShodanClient.Domain          → pure Shodan domain models, zero dependencies
ShodanClient.Application     → service & repository interfaces, options, exceptions
ShodanClient.Infrastructure  → HTTP repositories, routes, auth, resilience, rate limiting
ShodanClient                 → public facade, DI extensions, the package you install
```

Only `ShodanClient` is published to NuGet; the inner layers are merged into that single package so consumers add one
dependency.

### Sub-clients

| `IShodanClient.*` | Covers                                                                                                 |
|-------------------|--------------------------------------------------------------------------------------------------------|
| `.Hosts`          | `GET /shodan/host/{ip}`                                                                                |
| `.Search`         | `/shodan/host/search`, `/count`, `/search/facets\|filters\|tokens` (with auto-paging `SearchAllAsync`) |
| `.Scans`          | On-demand scanning: `/shodan/ports`, `/protocols`, `/scan`, `/scan/internet`, `/scans*`                |
| `.Alerts`         | Network alerts: CRUD, triggers, notifier links                                                         |
| `.Notifiers`      | Notification services CRUD                                                                             |
| `.Directory`      | Saved search query directory                                                                           |
| `.BulkData`       | Bulk datasets (Enterprise)                                                                             |
| `.Organization`   | Organization management (Enterprise)                                                                   |
| `.Account`        | The account linked to the API key                                                                      |
| `.Dns`            | Domain info, forward/reverse DNS                                                                       |
| `.Tools`          | HTTP header echo, caller IP                                                                            |
| `.ApiInfo`        | Plan info and remaining credits                                                                        |
| `.InternetDb`     | Key-less fast IP summaries                                                                             |
| `.Trends`         | Historical month-to-month search trends                                                                |
| `.Exploits`       | Exploit & vulnerability search                                                                         |
| `.Stream`         | Real-time banner firehose, as `IAsyncEnumerable<Banner>`                                               |

### Performance & correctness

- **Source-generated JSON** everywhere (`ShodanJsonContext`) — no reflection-based serialization,
  `JsonSerializerIsReflectionEnabledByDefault=false` enforces it at compile time.
- **Native AOT / trim compatible** — every project builds with `IsAotCompatible=true` and `EnableTrimAnalyzer=true`.
- **Per-surface `HttpClient`s** with `SocketsHttpHandler` connection pooling, `Microsoft.Extensions.Http.Resilience` (
  retry, circuit breaker, timeouts) on every surface except Streaming, which uses an infinite timeout instead.
- **Client-side rate limiting** via `System.Threading.RateLimiting` (token bucket, paced to Shodan's ~1 req/s default).
- **Clean base-URL ⇄ route separation** — routes never carry a host or API key; a single delegating handler injects
  `?key=` only on the surfaces that require it (all except InternetDB).

## Installation

```bash
dotnet add package ShodanClient
```

## Configuration

```jsonc
// appsettings.json
{
  "Shodan": {
    "ApiKey": "YOUR_SHODAN_API_KEY" // or set the Shodan__ApiKey environment variable
  }
}
```

```csharp
builder.Services.AddShodanClient(builder.Configuration);
// or: builder.Services.AddShodanClient(options => options.ApiKey = "...");
```

See [`Samples/ShodanClient.Samples.ConsoleApp`](Samples/ShodanClient.Samples.ConsoleApp) for a runnable example.

## Tools

[`Tools/ShodanClient.App`](Tools/ShodanClient.App) is an Avalonia desktop GUI built on top of the SDK, Windows-only for
now. It ships as Native AOT, self-contained `.zip` archives and `.msi` installers for `win-x64`/`win-arm64`, built and
published from the same [GitHub Release](https://github.com/AriusII/ShodanClient/releases) pipeline as the SDK
package, tagged `vX.Y.Z`. Each release also includes a `SHA256SUMS.txt` for integrity verification.

## License

[MIT](LICENSE)
