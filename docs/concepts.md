# Concepts

This page explains the parts of Fiks IO you meet when you use this client, and how they fit together. Read it if you are new to Fiks IO, or if a configuration value does not make sense.

## Fiks IO and Fiks Protokoll

[Fiks IO](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/) is a messaging service for the public sector in Norway, run by KS. A system sends a message to an account, and the system that owns the account receives it. Fiks IO is also the transport for [Fiks Protokoll](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/), where the message types and payloads follow a published protocol such as Fiks Arkiv or Fiks Plan.

The client hides the transport details:

- **Sending** goes through the Fiks IO REST API. The client looks up the receiver's public key, encrypts and signs the payload, and sets the [Fiks IO headers](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/#headere).
- **Receiving** goes through a [RabbitMQ](https://www.rabbitmq.com/) queue over AMQP. The client opens one connection, subscribes to your account's queue, and decrypts the payload when you ask for it.

## Account (konto)

An account (Norwegian: *konto*) is an address that can send and receive messages. It is identified by a GUID, the `kontoId`. Each account has its own queue and its own public key in the Fiks IO catalog (*katalog*).

A Fiks Protokoll account is an account that supports one or more protocols. It can only receive messages from accounts it has approved as senders.

## Integration (integrasjon)

An integration (Norwegian: *integrasjon*) is the identity your system uses towards Fiks. It has an `integrasjonId` (GUID) and a password, and must be given access to the account in FIKS-Konfigurasjon ([prod](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/), [test](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/)). The client sends the integration credentials with every request.

## Maskinporten

[Maskinporten](https://docs.digdir.no/docs/Maskinporten/maskinporten_autentisering.html) is Digdir's service for machine-to-machine authentication. The client gets an access token from Maskinporten and uses it for both the REST API and the AMQP connection. It renews the token when it is about to expire.

Your organisation needs a Maskinporten client (the `issuer` in this library is its client ID). The client authenticates to Maskinporten with either a business certificate (*virksomhetssertifikat*) or an RSA key pair registered on the Maskinporten client. See [Maskinporten](maskinporten.md).

## The three keys

These three are easy to mix up. They have different jobs and do not need to be related.

| Key | Used for | Configured with |
|---|---|---|
| Maskinporten certificate or key pair | Authenticating your system to Maskinporten | `WithMaskinportenConfiguration` or `WithMaskinportenKeyPairConfiguration` |
| The account's private key (PKCS#8) | Decrypting messages sent to your account. The matching public key is in the Fiks IO catalog, and senders encrypt with it | `WithFiksKontoConfiguration` |
| ASiC-E signing key | Signing the payload packages you send | `WithAsiceSigningConfiguration` |

You can reuse the business certificate for ASiC-E signing, but the account key pair is always separate. See [Keys and signing](keys-and-signing.md).

## Message (melding)

A message (Norwegian: *melding*) has:

- a **message type** (`meldingType`), a string the receiver uses to decide what to do, for example `no.ks.fiks.arkiv.v1.ping`
- a sender and a receiver account
- an optional **payload**: one or more files, packed into an [ASiC-E](https://docs.digdir.no/dpi_dokumentpakke_sikkerhet.html) container, signed and encrypted
- a time to live (`ttl`), 2 days by default
- optional headers, including `klientMeldingId` and `klientKorrelasjonsId`, which your own systems can use to track and correlate messages

A reply is a message with `svarPaMelding` set to the ID of the message it answers.

## Message lifecycle

1. The sender calls `Send`. The client encrypts the payload with the receiver's public key and posts it to the Fiks IO API, which returns a `SendtMelding` with the new `MeldingId`.
2. Fiks IO puts the message on the receiver's queue.
3. The receiver's client delivers the message to your handler as `MottattMeldingArgs`. Large payloads are fetched from Fiks Dokumentlager automatically.
4. Your handler processes the message and, if needed, replies with `SvarSender.Svar`.
5. Your handler acknowledges the message with `AckAsync`, `NackAsync` or `RejectWithRequeueAsync`. Until it does, the message stays on the queue as unacknowledged.

See [Sending and receiving](sending-and-receiving.md) for the code, and for what each acknowledgement does.
