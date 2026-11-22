# Changelog

All notable changes to `KS.Fiks.IO.Client` are documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses [semantic versioning](https://semver.org/).

Upgrading from 7.x? See the [migration guide](docs/migrating-to-v8.md).

## [8.0.0] - Unreleased

### Added
- Automatic upload of the account's public key to the Fiks IO catalog on startup. Pass the public certificate as the third argument to `WithFiksKontoConfiguration`, or as `offentligNokkel` to `KontoConfiguration`. New property `FiksIOClient.IsAutomaticPublicKeyUploadEnabled`. See [Automatic public key sync](docs/automatic-public-key-sync.md) (#371).
- Maskinporten authentication with your own RSA key pair instead of a business certificate: `FiksIOConfigurationBuilder.WithMaskinportenKeyPairConfiguration(rsa, issuer, keyIdentifier)` (#397).
- New package [KS.Fiks.IO.ProtokollKonfigurasjon.Client](KS.Fiks.IO.ProtokollKonfigurasjon.Client/README.md), a client for creating and managing Fiks Protokoll accounts (#365).
- New documentation under [docs/](docs/README.md), a [CONTRIBUTING.md](CONTRIBUTING.md) and this changelog.

### Changed

#### Breaking changes
- Dropped the `net8.0` target. The package targets `net10.0`, `netstandard2.1` and `netstandard2.0`. Applications on .NET 8 and 9 now use the `netstandard2.1` build. The builds have the same behaviour (#400).
- On `net10.0`, the package depends on Microsoft.Extensions.Logging and System.Text.Json 11.0.0. A `net10.0` application that references these packages directly in version 10.x fails to restore with `NU1605` (package downgrade). The `netstandard` builds stay on 10.0.15 (#400).
- `FiksIOConfigurationBuilder.WithApiConfiguration(host, port)` now applies the host and port. Before, the call was ignored. It throws `ArgumentException` if the host is null, empty or whitespace, and `ArgumentOutOfRangeException` if the port is not between 1 and 65535. With `BuildDevConfiguration(amqpHost, apiHost)`, the `apiHost` argument decides the host and only the port from `WithApiConfiguration` is used.
- `FiksIOClient.CreateAsync` now reads the account's public key from the catalog at startup and checks that a configured private key matches it. If none does, the client logs an `ERROR` and starts. With automatic public key upload enabled, it throws `InvalidOperationException` instead (#371).
- `ValidatePublicKeyAgainstPrivateKeyAsync()` now checks all configured private keys, not only the first. It returns `false` and logs a warning when no public key is registered, instead of throwing (#371).
- `KontoConfiguration` throws `ArgumentException` instead of `ArgumentNullException` when the list of private keys is empty, and now also throws `ArgumentException` when a private key is null or whitespace (#371).
- The public constructor of `AmqpConnectionManager` has a new `integrasjonId` parameter (#398).

#### Other changes
- When you create `FiksIOConfiguration` with its constructor and leave out `amqpConfiguration`, the AMQP host now follows the API host: the production API gives `io.fiks.ks.no` and the test API gives `io.fiks.test.ks.no`. Before, the API host was used as AMQP host. Other API hosts are unchanged.
- `FiksIOAmqpConnectionFailedException` and the log message for a failed AMQP connection now include the integration ID and the application name (#398).

### Fixed
- `SvarSender.Svar` with a string, stream or file payload now passes the `CancellationToken` on. Before, the token was ignored for these overloads.

### Dependencies
- KS.Fiks.IO.Send.Client 2.2.0 → 2.3.0
- Microsoft.Extensions.Logging 10.0.13 → 11.0.0 (`net10.0`) and 10.0.15 (`netstandard2.0`, `netstandard2.1`)
- System.Text.Json 10.0.13 → 11.0.0 (`net10.0`) and 10.0.15 (`netstandard2.0`, `netstandard2.1`)
- Microsoft.SourceLink.GitHub 10.0.402 → 11.0.100 (build-time only)

## [7.1.0] - 2026-10-21

### Added
- `RejectWithRequeueAsync()` on `ISvarSender` and `IAmqpAcknowledgeManager`. It puts the message back on the queue and increments its delivery count. When the maximum delivery count is reached, the message is moved to the dead letter queue and the sender receives an error message (#387).

### Changed

#### Breaking changes
- `NackWithRequeueAsync()` now behaves like `RejectWithRequeueAsync()`. Before, the message was redelivered until its TTL expired. The method is marked `[Obsolete]`. Use `RejectWithRequeueAsync()`.
- Custom implementations of `ISvarSender` or `IAmqpAcknowledgeManager`, such as hand-written test doubles, must implement `RejectWithRequeueAsync()`. Mocks created with Moq or similar are not affected.
- `Send` throws `ArgumentException` if two payloads have the same file name, if a file name contains `\`, or if it is `mimetype` or starts with `META-INF`.
- `DecryptedPayloads` rejects more malformed archives. These errors come from KS.Fiks.ASiC-E and are not wrapped in `FiksIODecryptionException`.

#### Other changes
- `NackAsync()` is unchanged: the message goes to the dead letter queue and no error message is sent to the sender.
- KS.Fiks.ASiC-E no longer writes log messages to the console.

### Fixed
- Sending a message, or reading a received message with `DecryptedPayloads`, no longer leaves a console logger thread running for each message (ks-no/fiks-asice-dotnet#323).

### Security
- System.Security.Cryptography.Xml and System.Security.Cryptography.Pkcs are now at least 10.0.13 (via KS.Fiks.ASiC-E 4.1.0). This fixes [GHSA-7hqp-7c5m-4vq6](https://github.com/advisories/GHSA-7hqp-7c5m-4vq6), [GHSA-c399-9853-6jmj](https://github.com/advisories/GHSA-c399-9853-6jmj), [GHSA-r955-8q5j-3h83](https://github.com/advisories/GHSA-r955-8q5j-3h83), [GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx) and [GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7).
- KS.Fiks.ASiC-E 4.1.0 fixes missing CMS signature verification (ks-no/fiks-asice-dotnet#316). The Fiks IO client does not call the ASiC-E verification API when it decrypts, so this does not add signature verification for received messages.

### Dependencies
- KS.Fiks.IO.Send.Client 2.1.1 → 2.2.0
- KS.Fiks.IO.Crypto 1.1.0 → 1.2.1 and KS.Fiks.ASiC-E 4.0.3 → 4.1.0 (transitive, via KS.Fiks.IO.Send.Client)
- System.Text.Json 10.0.12 → 10.0.13
- Microsoft.Extensions.Logging 10.0.12 → 10.0.13
- Microsoft.SourceLink.GitHub 10.0.401 → 10.0.402 (build-time only)

## [7.0.3] - 2026-09-14

### Added
- `net10.0` target.

### Fixed
- Failures when retrieving a Maskinporten token are thrown as `FiksIOMaskinportenTokenException`, with the original exception as inner exception (#369).
- The token is requested before the AMQP connection is opened, so a Maskinporten failure shows up as a token error instead of a generic RabbitMQ connection failure (#375). Code that catches specific exception types on connect may see the new type.

### Dependencies
- KS.Fiks.IO.Send.Client 2.1.0 → 2.1.1. This brings KS.Fiks.IO.Crypto 1.1.0 with KS.Fiks.ASiC-E 4.0.3 and KS.Fiks.Crypto 3.0.8, and replaces the ASiC-E 3.0.7-alpha that all 7.0.x versions resolved to. If you pinned KS.Fiks.ASiC-E, KS.Fiks.IO.Crypto or KS.Fiks.IO.Send.Client to work around the alpha dependency, you can remove the pin.
- RabbitMQ.Client 7.2.1 → 7.2.2, Microsoft.Extensions.Logging 10.0.6 → 10.0.12, System.Text.Json 10.0.6 → 10.0.12, Microsoft.SourceLink.GitHub 10.0.401 (build-time only).
- Transitive minimums raised: System.Security.Cryptography.Xml 10.0.10 (fixes 8 high-severity advisories), Microsoft.Extensions.Logging 10.0.9, MimeMapping 4.0.0, BouncyCastle.Cryptography 2.6.2.

## [7.0.2] - 2026-04-17

### Changed
- Updated dependencies.

## [7.0.1] - 2026-03-13

### Fixed
- `RabbitMQEventLogger` did not register RabbitMQ log events since version 6.0.0.

## [7.0.0] - 2026-03-02

### Added
- `IFiksIOClient.ValidatePublicKeyAgainstPrivateKeyAsync()`, which checks whether the public key in the Fiks IO catalog matches the configured private key (#335).

### Removed
- `IFiksIOClient.Lookup()`. The backend no longer supports this endpoint.

### Dependencies
- KS.Fiks.IO.Send.Client 2.0.7 → 2.1.0
- KS.Fiks.Maskinporten.Client 2.0.6 → 2.0.7
- RabbitMQ.Client 7.2.0 → 7.2.1
- Microsoft.Extensions.Logging 10.0.0 → 10.0.3
- System.Text.Json 10.0.0 → 10.0.3

## Earlier versions

See [GitHub releases](https://github.com/ks-no/fiks-io-client-dotnet/releases) for versions before 7.0.0.

[8.0.0]: https://github.com/ks-no/fiks-io-client-dotnet/compare/v7.1.0...main
[7.1.0]: https://github.com/ks-no/fiks-io-client-dotnet/releases/tag/v7.1.0
[7.0.3]: https://github.com/ks-no/fiks-io-client-dotnet/releases/tag/v7.0.3
[7.0.2]: https://github.com/ks-no/fiks-io-client-dotnet/releases/tag/v7.0.2
[7.0.1]: https://github.com/ks-no/fiks-io-client-dotnet/releases/tag/7.0.1
[7.0.0]: https://github.com/ks-no/fiks-io-client-dotnet/releases/tag/7.0.0
