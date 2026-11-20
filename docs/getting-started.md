# Getting started

In this tutorial you build a small console application that sends a `ping` message to your own Fiks IO account, receives it, and replies with `pong`. It takes about 30 minutes if you have the accounts in place.

You use the Fiks test environment throughout.

## What you need

Before you start, make sure you have:

- [ ] **A Fiks IO account** (*konto*) in the test environment, and its account ID. Set it up in [FIKS-Konfigurasjon (test)](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/).
- [ ] **An integration** (*integrasjon*) with access to the account, and its ID and password. Also set up in FIKS-Konfigurasjon.
- [ ] **A Maskinporten client** in Digdir's test environment with access to the `ks:fiks` scope, its client ID, and a business certificate (*virksomhetssertifikat*) registered on it.
- [ ] **The .NET 10 SDK**, and **OpenSSL** for creating keys.

If any of the terms are new, read [Concepts](concepts.md) first.

## 1. Create the project

```bash
dotnet new console -o FiksIOPing
cd FiksIOPing
dotnet add package KS.Fiks.IO.Client
```

## 2. Create the keys

You need two key pairs: one for the account, so others can encrypt messages to you, and one for signing the messages you send.

```bash
# Account key pair
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:4096 -out konto-private.pem
openssl req -new -x509 -key konto-private.pem -out konto-public.pem -days 99999

# ASiC-E signing key pair
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:4096 -out asice-private.pem
openssl req -new -x509 -key asice-private.pem -out asice-public.pem -days 99999
```

`openssl req` asks for certificate details. You can accept the defaults.

Upload `konto-public.pem` to your account in FIKS-Konfigurasjon (test). This is the key senders use to encrypt messages to you.

Copy your business certificate to the project folder as `virksomhetssertifikat.p12`.

## 3. Write the program

Replace the contents of `Program.cs` with this, and fill in your own values at the top:

```csharp
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;

var kontoId = Guid.Parse("<your account ID>");
var integrasjonId = Guid.Parse("<your integration ID>");
var integrasjonPassord = "<your integration password>";
var issuer = "<your Maskinporten client ID>";
var maskinportenCertificate = X509CertificateLoader.LoadPkcs12FromFile(
    "virksomhetssertifikat.p12", "<certificate password>");

// 1. Build the configuration for the test environment
var configuration = FiksIOConfigurationBuilder
    .Init()
    .WithAmqpConfiguration("fiks-io-ping-tutorial", 10)
    .WithMaskinportenConfiguration(maskinportenCertificate, issuer)
    .WithFiksIntegrasjonConfiguration(integrasjonId, integrasjonPassord)
    .WithFiksKontoConfiguration(kontoId, File.ReadAllText("konto-private.pem"))
    .WithAsiceSigningConfiguration("asice-public.pem", "asice-private.pem")
    .BuildTestConfiguration();

// 2. Create the client. It connects to Fiks IO and checks your keys.
await using var client = await FiksIOClient.CreateAsync(configuration);

// 3. Subscribe to messages: reply to "ping" with "pong", and acknowledge every message
await client.NewSubscriptionAsync(async args =>
{
    Console.WriteLine($"Received '{args.Melding.MeldingType}' with meldingId {args.Melding.MeldingId}");

    if (args.Melding.MeldingType == "ping")
    {
        await args.SvarSender.Svar("pong", "Hello back!", "pong.txt");
    }

    await args.SvarSender.AckAsync();
});

// 4. Send "ping" to your own account
var request = new MeldingRequest(
    avsenderKontoId: kontoId,
    mottakerKontoId: kontoId,
    meldingType: "ping");
var sendtMelding = await client.Send(request, "Hello, Fiks IO!", "ping.txt");
Console.WriteLine($"Sent 'ping' with meldingId {sendtMelding.MeldingId}");

Console.WriteLine("Press Enter to quit.");
Console.ReadLine();
```

Here is what the program does:

1. The builder fills in the hosts and Maskinporten endpoints for the test environment. `WithFiksKontoConfiguration` takes the private key as text, while `WithAsiceSigningConfiguration` takes file paths.
2. `CreateAsync` gets a Maskinporten token, checks that your private key matches the public key on the account, and connects to your account's queue.
3. The handler runs for each message. It must acknowledge every message, otherwise the message stays on the queue.
4. `Send` encrypts the payload with your account's public key and sends it. The receiver is your own account, so the message comes back to your handler.

## 4. Run it

```bash
dotnet run
```

You should see something like this:

```
Sent 'ping' with meldingId 3f0c...
Received 'ping' with meldingId 3f0c...
Received 'pong' with meldingId 9a41...
```

The `pong` message is your reply to the `ping`. Your handler acknowledges it without replying, so the exchange stops there.

If the client does not start, or no messages arrive, see [Troubleshooting](troubleshooting.md).

## Fiks Protokoll accounts

If your account is a Fiks Protokoll account, two things are different:

- Use a message type from the protocol, for example `no.ks.fiks.arkiv.v1.ping` and `no.ks.fiks.arkiv.v1.pong` for Fiks Arkiv.
- Add your own account as an approved sender in FIKS-Konfigurasjon, so it can send messages to itself.

## Next steps

- [Sending and receiving](sending-and-receiving.md): payload variants, acknowledgement, and correlation IDs
- [Configuration](configuration.md): production, AMQP settings, and all builder methods
- [Running in production](operations.md): client lifetime, health checks and logging
- [The example application](../ExampleApplication/README.md): a fuller program with a background service and logging
