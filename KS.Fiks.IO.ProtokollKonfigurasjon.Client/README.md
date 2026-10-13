# KS.Fiks.IO.ProtokollKonfigurasjon.Client

A .NET client for the [Fiks Protokoll Konfigurasjon API](https://developers.fiks.ks.no/api/fiks-protokoll-konfigurasjon-integrasjon-api-v1.json), used to manage Fiks Protokoll accounts (`konto`), systems, and protocol definitions. This is a thin, hand-written wrapper around an [NSwag](https://github.com/RicoSuter/NSwag)-generated client that adds Fiks integrasjon authentication (Maskinporten bearer token + integrasjon id/password headers) to every request.

This package is separate from [KS.Fiks.IO.Client](https://www.nuget.org/packages/KS.Fiks.IO.Client), which is used for sending and receiving Fiks-IO/Fiks-Protokoll messages. Use this package when you need to create or manage the Protokoll `konto` (account) itself, rather than sending messages through it.

## Installation
Install the `KS.Fiks.IO.ProtokollKonfigurasjon.Client` NuGet package in your .NET project.

## Usage

```csharp
using KS.Fiks.IO.ProtokollKonfigurasjon.Client;

IProtokollKonfigurasjonClient client = KontoKonfigurasjonClient.CreateClient(
    hostUrl: "https://api.fiks.test.ks.no",
    integrasjonId: integrasjonId,
    integrasjonPassord: integrasjonPassord,
    maskinportenTokenSupplier: async () => (await maskinportenClient.GetAccessToken(scope)).Token);

var request = new CreateProtokollKontoRequest
{
    Navn = "Min konto",
    Beskrivelse = "Opprettet via ProtokollKonfigurasjon.Client",
    StottetProtokollNavn = "no.ks.fiks.arkiv",
    Parts = new[] { new PartRequest { PartNavn = "arkiv.full", StottetProtokollVersjon = "v1" } },
    OffentligNokkel = offentligNokkel
};

var konto = await client.CreateKontoAsync(fiksOrgId, systemId, request);
```

`client` implements `IDisposable` and owns the `HttpClient` that `CreateClient` created for it, so dispose it (or wrap it in a `using`) once you are done with it. (If you instead construct `ProtokollKonfigurasjonClient` directly with your own `HttpClient`, that one is yours to manage - disposing the client leaves it open.)

### Authentication

`KontoKonfigurasjonClient.CreateClient` takes:
- `hostUrl` - base URL of the Fiks Protokoll Konfigurasjon API (`https://api.fiks.ks.no` in production, `https://api.fiks.test.ks.no` in test). Must be an absolute `https` URL - the integrasjon password and the Maskinporten token are sent to this host on every request, so `CreateClient` rejects relative and plaintext `http` URLs. The credentials are pinned to this URL's origin: the client refuses to send them anywhere else, even if the generated `BaseUrl` property is changed afterwards.
- `integrasjonId` / `integrasjonPassord` - your Fiks-IO integrasjon credentials.
- `maskinportenTokenSupplier` - a delegate that returns a valid Maskinporten access token. This is invoked on every outgoing HTTP request, so it should return a cached/memoized token rather than requesting a new one from Maskinporten on every call. `Ks.Fiks.Maskinporten.Client.MaskinportenClient.GetAccessToken` already caches internally, but it takes a scope and returns a token object - wrap it as in the example above (`async () => (await maskinportenClient.GetAccessToken(scope)).Token`).

### Available operations

The generated client (`IProtokollKonfigurasjonClient`) exposes operations for managing `konto`, `system`, and protocol definitions, including: `CreateKontoAsync`, `GetKontoAsync`, `KontoSokAsync`, `UpdateKontoOffentligNokkelAsync`, `UpdateParterAsync`, `GetSystemAsync`, `SystemSokAsync`, `GetFiksOrgsMedSystemerAsync`, `GetForespurteTilgangerPaaKontoAsync`, `GetSystemerMedTilgangTilKontoAsync`, `GetProtokollDefinisjonAsync`, `GetProtokollDefinisjonerAsync`, and `GetParterForProtokollAsync`. See the API's [OpenAPI specification](https://developers.fiks.ks.no/api/fiks-protokoll-konfigurasjon-integrasjon-api-v1.json) for full request/response details.

### Errors

API errors are surfaced as `ProtokollKonfigurasjonApiException`.

## Example

See [ExampleApplication](https://github.com/ks-no/fiks-io-client-dotnet/tree/main/ExampleApplication) in this repository for a full working example (press the **K-key** to create a `konto`).
