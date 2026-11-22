# Contributing

Thank you for helping improve the Fiks IO client. This page explains how to build and test the code.

## Repository layout

| Folder | Contents |
|---|---|
| `KS.Fiks.IO.Client` | The `KS.Fiks.IO.Client` package |
| `KS.Fiks.IO.Client.Tests` | Unit tests for the client |
| `KS.Fiks.IO.ProtokollKonfigurasjon.Client` | The `KS.Fiks.IO.ProtokollKonfigurasjon.Client` package |
| `KS.Fiks.IO.ProtokollKonfigurasjon.Client.Tests` | Unit tests for that package |
| `ExampleApplication` | A console application that uses both packages, see its [README](ExampleApplication/README.md) |
| `docs` | User documentation |

## Build and test

You need the .NET 10 SDK. The libraries target `net10.0`, `netstandard2.1` and `netstandard2.0`.

```bash
dotnet build KS.Fiks.IO.Client.sln
dotnet test KS.Fiks.IO.Client.sln
```

`ExampleApplication` references `KS.Fiks.IO.ProtokollKonfigurasjon.Client` as a project, so you do not need to pack anything or set up a local NuGet feed.

The tests use xUnit, Moq and Shouldly.

## Strong-name signing

The assemblies are [strong-named](https://learn.microsoft.com/en-us/dotnet/standard/assembly/strong-named) with `fiks-io-strongly-named-key.snk`. The `.snk` key format is based on SHA-1.

On Linux distributions where the OpenSSL 3 crypto policy rejects SHA-1 signatures (Fedora 38 and later, RHEL 9 and later), the build fails with `OpenSslCryptographicException: ... invalid digest`. Set this variable when you build:

```bash
OPENSSL_ENABLE_SHA1_SIGNATURES=1 dotnet test KS.Fiks.IO.Client.sln
```

Do not turn off signing with `/p:SignAssembly=false`. The libraries grant `InternalsVisibleTo` to the test projects by full public key, so unsigned test assemblies fail to compile (CS0281).

CI runs on Ubuntu, where no workaround is needed.

## Continuous integration

GitHub Actions builds and tests every push. Releases are built and published to nuget.org by the same workflow, started manually. Integration tests run in a separate repository.

## Documentation

- Add user-visible changes to [CHANGELOG.md](CHANGELOG.md), under the version that is not yet released. Mark breaking changes, and update the migration guide in `docs/` when users must change their code.
- The README is also shown on nuget.org. Keep it short, and put details in `docs/`.
- Each page in `docs/` has one purpose: a tutorial, a how-to, a reference or an explanation. See [docs/README.md](docs/README.md).
- Code examples must compile against the current version. Check them in a small console project that references `KS.Fiks.IO.Client/KS.Fiks.IO.Client.csproj`.
- Check the behaviour in the code before you document it.
