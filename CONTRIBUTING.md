# Contributing

Thanks for considering a contribution to ShodanClient.

## Project layout

```
Src/
  ShodanClient.Domain/          Domain models, zero dependencies
  ShodanClient.Application/     Service & repository interfaces, options, exceptions
  ShodanClient.Infrastructure/  HTTP repositories, route registries, auth, resilience
  ShodanClient/                 Public facade + DI extensions (the published package)
Tests/
  ShodanClient.Domain.Tests/
  ShodanClient.Application.Tests/
  ShodanClient.Infrastructure.Tests/
  ShodanClient.Tests/                  End-to-end facade / DI tests
  ShodanClient.ArchitectureTests/      Enforces the layer dependency rules
Samples/
  ShodanClient.Samples.ConsoleApp/
Benchmarks/
  ShodanClient.Benchmarks/
Tools/
  ShodanClient.App/              Avalonia desktop GUI built on the SDK (Windows-only)
```

## Dependency rule

`Domain` must never depend on `Application` or `Infrastructure`. `Application` must never depend on `Infrastructure`. This is enforced automatically by `ShodanClient.ArchitectureTests` — a PR that violates it will fail CI.

## Getting started

```bash
dotnet restore
dotnet build ShodanClient.slnx
dotnet test ShodanClient.slnx
dotnet format --verify-no-changes
```

## Adding a NuGet package

This solution uses [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management). Add packages with `dotnet add package <Id>` from within the target project — the version is recorded centrally in `Directory.Packages.props`, never in individual `.csproj` files. This applies to `Tools/ShodanClient.App` as well: never add an inline `Version=` to a `<PackageReference>` there.

## Pull requests

- Keep PRs focused; one logical change per PR.
- Add or update tests for any behavior change.
- `dotnet format --verify-no-changes` and `dotnet test` must pass locally before opening a PR.
- Update `CHANGELOG.md` under `[Unreleased]`.
