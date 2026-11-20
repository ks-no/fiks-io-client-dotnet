# Maskinporten

How to set up authentication with [Maskinporten](https://docs.digdir.no/docs/Maskinporten/maskinporten_autentisering.html). The client uses the Maskinporten token for both the Fiks IO API and the AMQP connection.

You need a Maskinporten client for your organisation. Its client ID is the `issuer` in this library. The client authenticates with one of these:

| Option | Builder method | When to use |
|---|---|---|
| Business certificate | `WithMaskinportenConfiguration(certificate, issuer)` | You have a *virksomhetssertifikat* registered on the Maskinporten client |
| Business certificate with key identifier | `WithMaskinportenConfiguration(certificate, issuer, keyIdentifier)` | As above, and you want the token request to include a `kid` |
| RSA key pair | `WithMaskinportenKeyPairConfiguration(rsa, issuer, keyIdentifier)` | You have registered your own public key on the Maskinporten client instead of using a certificate |

You can only use one of them. The build methods throw `ArgumentException` if you set both a certificate and a key pair.

## Business certificate

```csharp
var maskinportenCertificate = X509CertificateLoader.LoadPkcs12FromFile("/path/to/virksomhetssertifikat.p12", "password");

var configuration = FiksIOConfigurationBuilder
    .Init()
    .WithMaskinportenConfiguration(maskinportenCertificate, issuer)
    // ...
    .BuildProdConfiguration();
```

`X509CertificateLoader` is available from .NET 9. On older platforms, use the `X509Certificate2` constructor or load the certificate from the certificate store.

If you have a key identifier for the certificate, use the overload with `keyIdentifier`. Read more about [`kid` in the token request](https://docs.digdir.no/docs/Maskinporten/maskinporten_protocol_jwtgrant.html).

## RSA key pair

Instead of a business certificate, the client can authenticate with your own RSA key pair registered on the Maskinporten client.

### 1. Generate a key pair

```bash
openssl genrsa -out maskinporten-private.pem 2048
openssl rsa -in maskinporten-private.pem -pubout -out maskinporten-public.pem
```

The key must be at least 2048 bits.

### 2. Register the public key on your Maskinporten client

- In the self-service web: [Registrere nøkkel på klient](https://docs.digdir.no/docs/Maskinporten/maskinporten_sjolvbetjening_web.html#registrere-nøkkel-på-klient). The web requires the key as a JWK, so convert the PEM file to JWK first.
- Through the self-service API: [Registrere klient som bruker egen nøkkel](https://docs.digdir.no/docs/Maskinporten/maskinporten_sjolvbetjening_api.html#registrere-klient-som-bruker-egen-nøkkel).

Note the key identifier (`kid`) the key is registered with. Maskinporten requires it when you do not use a certificate.

### 3. Configure the client

```csharp
// Keep this object alive as long as the FiksIOClient is in use. Do not wrap it in "using".
var maskinportenKeyPair = RSA.Create();
maskinportenKeyPair.ImportFromPem(File.ReadAllText("maskinporten-private.pem"));

var configuration = FiksIOConfigurationBuilder
    .Init()
    .WithMaskinportenKeyPairConfiguration(maskinportenKeyPair, issuer, keyIdentifier)
    // ...
    .BuildProdConfiguration();
```

- **`keyIdentifier` is required.** The build methods throw `ArgumentException` without it.
- **You own the `RSA` object.** Neither the builder nor the client disposes it. If it is disposed while the client runs, token renewal fails with `ObjectDisposedException`.
- **`RSA.ImportFromPem` requires .NET 5 or later.** On .NET Framework, load the key another way, for example by reading the PEM file into `RSAParameters` and calling `ImportParameters`.
- **ASiC-E signing is still required** and is set up separately. See [Keys and signing](keys-and-signing.md#asic-e-signing).

### Rotate the key

A key is valid for at most one year, see [Maskinporten authentication](https://docs.digdir.no/docs/Maskinporten/maskinporten_autentisering.html). To rotate:

1. Generate a new key pair and register the public key with a new `kid`.
2. Build a new configuration with the new key and `kid`, and create a new `FiksIOClient`.
3. Dispose the old client, then the old `RSA` object.
