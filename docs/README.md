# KS.Fiks.IO.Client documentation

Documentation for [KS.Fiks.IO.Client](https://www.nuget.org/packages/KS.Fiks.IO.Client), the .NET client for sending and receiving messages through Fiks IO.

## Learn

- [Getting started](getting-started.md): build a console application that sends and receives a message, step by step
- [Concepts](concepts.md): accounts, integrations, Maskinporten, the three keys, and the message lifecycle

## How to

- [Sending and receiving](sending-and-receiving.md): send, receive, reply, acknowledge, and look up accounts
- [Maskinporten](maskinporten.md): authenticate with a business certificate or an RSA key pair, and rotate keys
- [Keys and signing](keys-and-signing.md): create the account key, set up ASiC-E signing, and provide public keys yourself
- [Running in production](operations.md): client lifetime, health checks, logging and shutdown
- [Troubleshooting](troubleshooting.md): common errors and how to fix them

## Reference

- [Configuration](configuration.md): builder methods, default values per environment, AMQP settings, and configuration without the builder
- [Automatic public key sync](automatic-public-key-sync.md): what happens at startup when the client checks and uploads the account's public key

## Elsewhere

- [Example application](../ExampleApplication/README.md): a console application that sends, receives and replies
- [KS.Fiks.IO.ProtokollKonfigurasjon.Client](../KS.Fiks.IO.ProtokollKonfigurasjon.Client/README.md): create and manage Fiks Protokoll accounts
- [Contributing](../CONTRIBUTING.md): build and test the library
- [Fiks IO on the Fiks platform (Norwegian)](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/)
