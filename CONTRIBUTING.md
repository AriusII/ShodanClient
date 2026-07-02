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

## Releasing

The SDK (NuGet package) and `Tools/ShodanClient.App` are released together, from a single tag, by
`.github/workflows/release.yml`. There are two ways to trigger it:

- **Push a tag**: `git tag v1.4.0 && git push origin v1.4.0`.
- **Actions UI**: go to Actions → Release → "Run workflow", type the version (e.g. `1.4.0`, no leading
  `v`), and run it from `main`. This only creates and pushes the `vX.Y.Z` tag — you'll see a short
  `dispatch-tag` run complete, immediately followed by a second, full run triggered by that tag push.
  Seeing two runs is expected, not a failure.

Either path then runs the same pipeline: build + test, pack and publish `ShodanClient` to NuGet.org, and
publish `ShodanClient.App` for `win-x64`/`win-arm64` as AOT `.zip` archives and `.msi` installers,
culminating in one GitHub Release with the `.nupkg`/`.snupkg`, `.zip`s, `.msi`s and a `SHA256SUMS.txt`.

Publishing to NuGet.org requires manual approval: the `publish-nuget` job is gated behind the repo's
"Release" GitHub environment, so a required reviewer must approve it before anything reaches NuGet.org or
becomes a public release.

Triggering a release via the Actions UI requires a repository secret named `RELEASE_TAG_TOKEN` (a
fine-grained PAT scoped to this repo with "Contents: Read and write") — the default `GITHUB_TOKEN` cannot
be used here because GitHub does not let tags pushed with it trigger further workflow runs. This PAT
expires periodically and needs rotating; if the "Run workflow" path starts failing at the tag-creation
step, that's the first thing to check. Pushing a tag manually with `git push` never depends on this token.
