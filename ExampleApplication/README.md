# Example application

A console application that shows how to use [KS.Fiks.IO.Client](../README.md). It subscribes to messages on your account, sends `ping` messages to itself, and replies to each `ping` with a `pong`. It also shows logging, health checks, and how to create a Fiks Protokoll account with [KS.Fiks.IO.ProtokollKonfigurasjon.Client](../KS.Fiks.IO.ProtokollKonfigurasjon.Client/README.md).

The application always runs against the Fiks test environment.

If you want to build your first application step by step instead, see [Getting started](../docs/getting-started.md).

## Run the application

1. You need a Fiks IO or Fiks Protokoll account in the test environment, with an integration, a Maskinporten client and the keys described in [Getting started](../docs/getting-started.md#what-you-need). See also [how to get started with Fiks Protokoll (Norwegian)](https://developers.fiks.ks.no/tjenester/fiksprotokoll/#hvordan-komme-i-gang-med-fiks-protokoll).
2. Create `appsettings.Test.json` in this folder with your own values, see [Settings](#settings). The file is in `.gitignore`, so your secrets are not committed. It overrides `appsettings.json`.
3. Run the application from this folder, so it finds the settings files and `testfile.txt`:

   ```bash
   cd ExampleApplication
   dotnet run
   ```

4. Press Enter to send a `ping` to your own account. The log shows the `ping` arriving, the `pong` reply being sent, and the `pong` arriving.

If you use a Fiks Protokoll account, add your own account as an approved sender in FIKS-Konfigurasjon, and use the protocol keys (A, P or M) instead of Enter.

## Keys

| Key | What it does |
|---|---|
| Enter | Sends a Fiks IO `ping` to your own account |
| A | Sends a Fiks Arkiv `ping` (`no.ks.fiks.arkiv.v1.ping`) |
| P | Sends a Fiks Plan `ping` (`no.ks.fiks.plan.v2.ping`) |
| M | Sends a Fiks Matrikkelføring `ping` (`no.ks.fiks.matrikkelfoering.v2.ping`) |
| L | Logs the status: whether the connection is open (`IsOpenAsync`), the number of consumers and unread messages on the account (`GetKontoStatus`), and whether the private key matches the public key in the catalog (`ValidatePublicKeyAgainstPrivateKeyAsync`) |
| T | Fetches a Maskinporten token and logs it, see [Maskinporten token and API calls](#maskinporten-token-and-api-calls) |
| N | Creates a Fiks Arkiv account on the system given by `SystemId` |
| C | Sends an access request to the account created with N |
| V | Lists the access requests on the account created with N |
| R | Approves all access requests on the account created with N |
| Q | Quits |

The `ping` messages carry `testfile.txt` as payload, and a new `klientMeldingId` and `klientKorrelasjonsId`. When the application receives a `ping`, it logs the payload text and replies with the matching `pong`. It acknowledges every message it receives.

### Creating a Fiks Arkiv account

Use the keys N, C, V and R in this order:

1. **N** creates the account. The application remembers its ID for C, V and R until you quit.
2. **C** sends an access request from your system to the new account.
3. **V** lists the access requests.
4. **R** approves them.

## Settings

The application reads `appsettings.json` and then `appsettings.Test.json` from the folder you run it from. Paths in the settings are relative to the same folder.

| Setting | Description |
|---|---|
| `FiksIoAccountId` | Your account ID. The application sends `ping` messages to this account |
| `FiksIoIntegrationId` | Your integration ID |
| `FiksIoIntegrationPassword` | Your integration password |
| `FiksIoPrivateKey` | Path to the account's private key (PKCS#8). Not the Maskinporten key, see [Keys and signing](../docs/keys-and-signing.md#the-account-key) |
| `MaskinPortenIssuer` | Your Maskinporten client ID |
| `MaskinPortenCompanyCertificatePath` | Path to your business certificate (`.p12`) for Maskinporten |
| `MaskinPortenCompanyCertificatePassword` | Password for the certificate |
| `AsiceSigningPublicKey` | Path to the public certificate for ASiC-E signing |
| `AsiceSigningPrivateKey` | Path to the private key for ASiC-E signing |
| `ApiHost` | Host for the Fiks Protokoll Konfigurasjon API, used by the account management keys (N, C, V, R). Default `api.fiks.test.ks.no` |
| `ApiPort` | Port for the same API. Default `443` |
| `ApiScheme` | Scheme for the same API. Default `https` |
| `SystemId` | The system that will own the account created with N. Find it under your organisation in FIKS-Konfigurasjon ([test](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/)). The organisation is read from the account given by `FiksIoAccountId` |
| `ProtokollPublicKey` | Optional. Path to a public certificate to register on the account created with N |

`ApiHost`, `ApiPort` and `ApiScheme` do not affect `FiksIOClient`. You do not configure its API host, AMQP host, Maskinporten endpoints or integration scope: `BuildTestConfiguration()` sets them for the test environment, see [Default values per environment](../docs/configuration.md#default-values-per-environment).

## Where in the code

| What | Where |
|---|---|
| Build the configuration with the builder | [FiksIO/FiksIOConfigurationBuilder.cs](FiksIO/FiksIOConfigurationBuilder.cs), `CreateTestConfiguration` |
| Build the configuration without the builder | [FiksIO/FiksIOConfigurationBuilder.cs](FiksIO/FiksIOConfigurationBuilder.cs), `CreateConfig` |
| Create the client, set up logging and `RabbitMQEventLogger`, and run as a hosted service | [Program.cs](Program.cs), `Main` |
| Subscribe, reply, acknowledge, and read the payload with `AsiceReader` | [FiksIO/FiksIOSubscriber.cs](FiksIO/FiksIOSubscriber.cs) |
| Send with `klientMeldingId` and `klientKorrelasjonsId` | [FiksIO/MessageSender.cs](FiksIO/MessageSender.cs) |
| Health checks (L key) | [Program.cs](Program.cs), `WriteHeartBeatConnectionStatusToLog` and `WriteStatusFromApiToLog` |
| Fetch a Maskinporten token (T key) | [Program.cs](Program.cs), `WriteMaskinportenToken` |
| Create and manage a Fiks Arkiv account (N, C, V, R keys) | [Program.cs](Program.cs), `CreateFiksArkivKontoAsync` and the methods after it |
| The settings | [AppSettings/AppSettings.cs](AppSettings/AppSettings.cs) and [appsettings.json](appsettings.json) |

## Maskinporten token and API calls

Press **T** to fetch a Maskinporten token. If it works, your Maskinporten configuration is correct. You can then use the token to call the Fiks IO API by hand, for example to fetch your account:

```bash
curl -X 'GET' \
  'https://api.fiks.test.ks.no/fiks-protokoll/katalog/api/v1/kontoer/<FiksIoAccountId>' \
  -H 'accept: application/json' \
  -H 'Authorization: Bearer <maskinporten-token>' \
  -H 'IntegrasjonId: <FiksIoIntegrationId>' \
  -H 'IntegrasjonPassord: <FiksIoIntegrationPassword>'
```

Replace:

- `<FiksIoAccountId>` with the account you want to fetch
- `<maskinporten-token>` with the token from the T key
- `<FiksIoIntegrationId>` and `<FiksIoIntegrationPassword>` with your integration ID and password

The token is valid for 2 minutes. Press T again to get a new one.

## Building

`ExampleApplication` references both libraries as projects, so you do not need to pack anything first. For build problems, see [CONTRIBUTING.md](../CONTRIBUTING.md).
