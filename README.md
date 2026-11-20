# fiks-io-client-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.client.svg)](https://www.nuget.org/packages/KS.Fiks.IO.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/fiks-io-client-dotnet.svg)](https://github.com/ks-no/fiks-io-client-dotnet/issues)

A .NET client for sending and receiving messages through [Fiks IO](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/), the messaging service for the Norwegian public sector, and [Fiks Protokoll](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/), which runs on top of it.

## What you can do

The client handles Maskinporten authentication, encryption, signing and the Fiks IO headers for you. With it you can:

- [send messages](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/sending-and-receiving.md#send-a-message) with files, strings or streams as payload
- [receive, reply to and acknowledge messages](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/sending-and-receiving.md#receive-messages) on your account's queue
- [authenticate with Maskinporten](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/maskinporten.md) using a business certificate or your own RSA key pair
- [upload your account's public key automatically](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/automatic-public-key-sync.md), and rotate it without losing messages
- [monitor the connection](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/operations.md#health-checks) and log what happens

## Getting started

### Prerequisites

- A Fiks IO account (*konto*) with an integration (*integrasjon*) that has access to it. Set them up in FIKS-Konfigurasjon ([prod](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/), [test](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/)).
- A Maskinporten client with access to the `ks:fiks` scope, and a business certificate or RSA key pair registered on it.
- A key pair for the account. The public key is registered on the account, and the private key (PKCS#8) decrypts the messages you receive.
- A key pair or certificate for signing the messages you send (ASiC-E).

### Install

```bash
dotnet add package KS.Fiks.IO.Client
```

### Configure and create the client

```csharp
var configuration = FiksIOConfigurationBuilder
    .Init()
    .WithAmqpConfiguration("my-application", 10)
    .WithMaskinportenConfiguration(maskinportenCertificate, issuer)
    .WithFiksIntegrasjonConfiguration(integrasjonId, integrasjonPassord)
    .WithFiksKontoConfiguration(kontoId, File.ReadAllText("konto-private.pem"))
    .WithAsiceSigningConfiguration("asice-public.pem", "asice-private.pem")
    .BuildProdConfiguration(); // or BuildTestConfiguration()

var client = await FiksIOClient.CreateAsync(configuration);
```

For Maskinporten, use one of these:

```csharp
.WithMaskinportenConfiguration(maskinportenCertificate, issuer)                        // business certificate
.WithMaskinportenConfiguration(maskinportenCertificate, issuer, keyIdentifier)         // business certificate with kid
.WithMaskinportenKeyPairConfiguration(maskinportenKeyPair, issuer, keyIdentifier)      // your own RSA key pair
```

For ASiC-E signing, use one of these:

```csharp
.WithAsiceSigningConfiguration("asice-public.pem", "asice-private.pem") // key pair, as file paths
.WithAsiceSigningConfiguration(asiceCertificate)                        // X509Certificate2 with private key
```

For all builder methods and default values, see [Configuration](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/configuration.md).

### Send a message

```csharp
var request = new MeldingRequest(
    avsenderKontoId: kontoId,
    mottakerKontoId: receiverKontoId,
    meldingType: "no.ks.fiks.arkiv.v1.ping");

await client.Send(request, "/path/to/file.pdf");
```

### Receive and acknowledge messages

```csharp
await client.NewSubscriptionAsync(async args =>
{
    if (args.Melding.HasPayload)
    {
        await args.Melding.WriteDecryptedZip("/path/to/received.zip");
    }

    await args.SvarSender.AckAsync();
});
```

For a step-by-step tutorial, see [Getting started](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/getting-started.md). For replies, payload variants and correlation IDs, see [Sending and receiving](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/sending-and-receiving.md).

## Important to know

**Use one long-lived client.** Create the client when your application starts and keep it until it stops. Each client opens a connection to RabbitMQ, and creating clients often harms the server through [high connection churn](https://www.rabbitmq.com/connections.html#high-connection-churn). See [Running in production](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/operations.md).

**There are three different keys.** They have different jobs and do not need to be related:

| Key | Used for |
|---|---|
| Maskinporten certificate or key pair | Authenticating your system |
| The account's private key (PKCS#8) | Decrypting messages sent to you. The public key is in the Fiks IO catalog |
| ASiC-E signing key | Signing the messages you send |

See [Concepts](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/concepts.md#the-three-keys) and [Keys and signing](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/keys-and-signing.md).

**Acknowledge every message.** Call `AckAsync`, `NackAsync` or `RejectWithRequeueAsync` for each message you receive. Unacknowledged messages stay on the queue and block new ones. See [Acknowledge messages](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/sending-and-receiving.md#acknowledge-messages).

## Documentation

- [Getting started](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/getting-started.md): a tutorial from zero to sending and receiving
- [Concepts](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/concepts.md): accounts, integrations, Maskinporten, keys and the message lifecycle
- [Sending and receiving](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/sending-and-receiving.md)
- [Configuration](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/configuration.md)
- [Maskinporten](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/maskinporten.md)
- [Keys and signing](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/keys-and-signing.md)
- [Automatic public key sync](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/automatic-public-key-sync.md)
- [Running in production](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/operations.md)
- [Troubleshooting](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/docs/troubleshooting.md)

### Supported platforms

The package targets `net10.0`, `netstandard2.1` and `netstandard2.0`. The NuGet package is signed with a KS certificate, and the assemblies are [strong-named](https://learn.microsoft.com/en-us/dotnet/standard/assembly/strong-named).

### Related packages

[KS.Fiks.IO.ProtokollKonfigurasjon.Client](https://www.nuget.org/packages/KS.Fiks.IO.ProtokollKonfigurasjon.Client) is a client for the Fiks Protokoll Konfigurasjon API, used to create and manage Fiks Protokoll accounts. It is published from this repository. See its [README](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/KS.Fiks.IO.ProtokollKonfigurasjon.Client/README.md).

### Example application

[ExampleApplication](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/ExampleApplication/README.md) is a console application that subscribes to your account, sends `ping` messages to itself and replies with `pong`. It also shows logging, health checks and account management.

## Contributing

See [CONTRIBUTING.md](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/CONTRIBUTING.md) for how to build and test the library. Report bugs and suggestions as [GitHub issues](https://github.com/ks-no/fiks-io-client-dotnet/issues).

## License

[MIT](https://github.com/ks-no/fiks-io-client-dotnet/blob/main/LICENSE)
