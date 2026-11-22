# Migrating from 7.x to 8.0

Version 8.0 drops the `net8.0` target, moves `net10.0` to the 11.0 Microsoft packages, and adds automatic public key sync and Maskinporten with RSA key pairs. Most applications upgrade without code changes. This guide lists what can affect you, and what to do.

For the full list of changes, see the [changelog](../CHANGELOG.md).

## Checklist

| Change | You are affected if | What to do |
|---|---|---|
| [Microsoft packages 11.0 on net10.0](#1-update-microsoft-packages-on-net100) | Your application targets `net10.0` and references Microsoft.Extensions.\* or System.Text.Json 10.x directly | Update them to 11.0.0 |
| [The net8.0 target is gone](#2-net80-and-net90-applications) | Your application targets .NET 8 or 9 | Nothing |
| [`WithApiConfiguration` now works](#3-check-calls-to-withapiconfiguration) | You call `WithApiConfiguration` | Check that the host and port are right, and that they are set |
| [Key check at startup](#4-check-the-log-after-the-first-start) | Everyone | Check the log after the first start |
| [`ValidatePublicKeyAgainstPrivateKeyAsync`](#5-validatepublickeyagainstprivatekeyasync) | You call it | Check how you handle a missing key |
| [`KontoConfiguration` exceptions](#6-kontoconfiguration-exceptions) | You catch `ArgumentNullException` from `KontoConfiguration` | Catch `ArgumentException` |
| [Default AMQP host](#7-fiksioconfiguration-without-amqpconfiguration) | You create `FiksIOConfiguration` with its constructor, without `amqpConfiguration` | Usually nothing, see the section |
| [Cancellation in replies](#8-cancellation-in-replies) | You pass a `CancellationToken` to `Svar` | Nothing, unless you relied on it being ignored |
| [`AmqpConnectionManager` constructor](#9-amqpconnectionmanager) | You create `AmqpConnectionManager` yourself | Pass the integration ID |

## 1. Update Microsoft packages on net10.0

On `net10.0`, the client depends on Microsoft.Extensions.Logging and System.Text.Json 11.0.0. If your application references one of them directly in version 10.x, restore fails:

```
error NU1605: Warning As Error: Detected package downgrade: Microsoft.Extensions.Logging from 11.0.0 to 10.0.15.
```

Update the references to 11.0.0. It is a good idea to update the other Microsoft.Extensions.\* packages you use to 11.0.0 at the same time, so they stay on the same version.

## 2. net8.0 and net9.0 applications

The package no longer has a `net8.0` build. Applications on .NET 8 and 9 use the `netstandard2.1` build, which behaves the same way and stays on the 10.0 Microsoft packages. You do not need to change anything.

## 3. Check calls to WithApiConfiguration

In 7.x, `FiksIOConfigurationBuilder.WithApiConfiguration(host, port)` did nothing. In 8.0 it overrides the API host and port for the environment you build for.

If you call it, check that the values match the environment. For example, a call with `api.fiks.test.ks.no` followed by `BuildProdConfiguration()` now sends API calls to the test environment, while AMQP and Maskinporten stay on production. If you do not need to override the host, remove the call.

`WithApiConfiguration` also checks its arguments. It throws `ArgumentException` if the host is null, empty or whitespace, and `ArgumentOutOfRangeException` if the port is not between 1 and 65535. If you read the values from configuration, make sure they are set in every environment, or skip the call when they are not.

With `BuildDevConfiguration(amqpHost, apiHost)`, the `apiHost` argument decides the API host. If you also call `WithApiConfiguration`, only its port is used.

## 4. Check the log after the first start

`FiksIOClient.CreateAsync` now reads the account's public key from the Fiks IO catalog and checks that one of your private keys matches it. If none does, the client logs this and starts anyway:

```
ERROR No configured private key can decrypt messages for account {KontoId}. ...
```

The client could not decrypt incoming messages in 7.x either with this configuration, but now the problem shows up at startup. Configure the right private key for the account, or register the right public key. See [Keys and signing](keys-and-signing.md#the-account-key).

If the catalog is unavailable at startup, the client logs a warning and starts. If no key is registered, it starts without checking.

If you enable [automatic public key sync](automatic-public-key-sync.md), a mismatch stops the client with `InvalidOperationException` instead.

## 5. ValidatePublicKeyAgainstPrivateKeyAsync

`ValidatePublicKeyAgainstPrivateKeyAsync()` has changed in two ways:

- It checks all configured private keys, not only the first. It returns `true` if one of them matches.
- When no public key is registered for the account, it returns `false` and logs a warning. In 7.x, the exception from the catalog lookup was thrown.

If you caught that exception to detect a missing key, check for `false` instead.

## 6. KontoConfiguration exceptions

`KontoConfiguration` now throws `ArgumentException` when the list of private keys is empty. In 7.x it threw `ArgumentNullException`. It also throws `ArgumentException` when a private key is null, empty or whitespace. A null list still throws `ArgumentNullException`.

If you catch `ArgumentNullException` around `KontoConfiguration` or `WithFiksKontoConfiguration`, catch `ArgumentException` instead. `ArgumentNullException` is a subclass of `ArgumentException`, so catching `ArgumentException` covers both.

## 7. FiksIOConfiguration without amqpConfiguration

If you create `FiksIOConfiguration` with its constructor and leave out `amqpConfiguration`, the AMQP host now follows the API host:

| API host | AMQP host in 7.x | AMQP host in 8.0 |
|---|---|---|
| `api.fiks.ks.no`, or no `apiConfiguration` | `api.fiks.ks.no` | `io.fiks.ks.no` |
| `api.fiks.test.ks.no` | `api.fiks.test.ks.no` | `io.fiks.test.ks.no` |
| Any other host | The API host | The API host |

The old value was not an AMQP server, so this only fixes configurations that could not connect. If you use the builder, or pass `amqpConfiguration`, nothing changes.

## 8. Cancellation in replies

`SvarSender.Svar` with a string, stream or file payload now passes the `CancellationToken` on. In 7.x it was ignored for these overloads, so a cancelled token did not stop the reply. Now the reply is cancelled.

## 9. AmqpConnectionManager

The public constructor of `AmqpConnectionManager` has a new `integrasjonId` parameter after `amqpConfiguration`. The client creates this class itself, so this only affects you if you create it in your own code. Pass your integration ID.

The exception `FiksIOAmqpConnectionFailedException` now includes the integration ID and application name in its message. If you parse the message, update the parsing.

## Upgrading from 7.0.x

If you upgrade from 7.0.x, also read the breaking changes in [7.1.0](../CHANGELOG.md#710---2026-10-21):

- `NackWithRequeueAsync()` now behaves like `RejectWithRequeueAsync()`, and is obsolete. Use `RejectWithRequeueAsync()`.
- Hand-written implementations of `ISvarSender` or `IAmqpAcknowledgeManager` must implement `RejectWithRequeueAsync()`.
- `Send` rejects duplicate file names, file names with `\`, and the names `mimetype` and `META-INF*`.

## New features you can start using

These are optional:

- [Automatic public key sync](automatic-public-key-sync.md): the client registers the account's public key in the catalog, and you can rotate the key without losing messages.
- [Maskinporten with an RSA key pair](maskinporten.md#rsa-key-pair), instead of a business certificate.
- [KS.Fiks.IO.ProtokollKonfigurasjon.Client](../KS.Fiks.IO.ProtokollKonfigurasjon.Client/README.md): create and manage Fiks Protokoll accounts from code.
