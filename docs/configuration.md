# Configuration

Reference for `FiksIOConfiguration`: the builder, the default values for each environment, and how to set everything yourself.

## The builder

`FiksIOConfigurationBuilder` is the easiest way to build a configuration. You give it the values that are specific to you, and it fills in the hosts and endpoints for the environment.

```csharp
var configuration = FiksIOConfigurationBuilder
    .Init()
    .WithAmqpConfiguration("my-application", 10)
    .WithMaskinportenConfiguration(maskinportenCertificate, issuer)
    .WithFiksIntegrasjonConfiguration(integrasjonId, integrasjonPassord)
    .WithFiksKontoConfiguration(kontoId, kontoPrivateKeyPem)
    .WithAsiceSigningConfiguration("/path/to/asice-public.pem", "/path/to/asice-private.pem")
    .BuildProdConfiguration(); // or BuildTestConfiguration()

var client = await FiksIOClient.CreateAsync(configuration);
```

### Builder methods

| Method | Required | Description |
|---|---|---|
| `WithMaskinportenConfiguration(certificate, issuer)` | One of the Maskinporten methods | Authenticate with a business certificate. `issuer` is your Maskinporten client ID |
| `WithMaskinportenConfiguration(certificate, issuer, keyIdentifier)` | | As above, with a key identifier (`kid`) |
| `WithMaskinportenKeyPairConfiguration(rsa, issuer, keyIdentifier)` | | Authenticate with an RSA key pair. `keyIdentifier` is required. See [Maskinporten](maskinporten.md) |
| `WithFiksIntegrasjonConfiguration(integrasjonId, password)` | Yes | Your integration ID and password |
| `WithFiksKontoConfiguration(kontoId, privateKeyPem)` | Yes | Your account ID and its private key as PEM text (PKCS#8). Not a file path |
| `WithFiksKontoConfiguration(kontoId, privateKeyPems)` | | Several private keys, for key rotation |
| `WithFiksKontoConfiguration(kontoId, privateKeyPem(s), publicCertPem)` | | Also uploads the public key to the catalog. See [Automatic public key sync](automatic-public-key-sync.md) |
| `WithAsiceSigningConfiguration(publicCertPath, privateKeyPath)` | One of the ASiC-E methods | Sign with a key pair. Takes **file paths** |
| `WithAsiceSigningConfiguration(certificate)` | | Sign with an `X509Certificate2` that contains the private key |
| `WithAmqpConfiguration(applicationName, prefetchCount)` | No, but recommended | See [AMQP](#amqp) |
| `WithApiConfiguration(host, port)` | No | Overrides the API host and port. The scheme is always `https` |
| `BuildProdConfiguration()` | | Builds for production |
| `BuildTestConfiguration()` | | Builds for the test environment |
| `BuildDevConfiguration(amqpHost, apiHost)` | | Builds with your own AMQP and API hosts, and Maskinporten test. If you also call `WithApiConfiguration`, it overrides `apiHost` |

The build methods throw `ArgumentException` if a required part is missing, if you set both a Maskinporten certificate and a key pair, or if you use a key pair without `keyIdentifier`.

`WithFiksKontoConfiguration` takes the private key as PEM **text**, while `WithAsiceSigningConfiguration` takes **file paths**. To load the account key from a file, use `File.ReadAllText(path)`.

## Default values per environment

| Setting | Prod | Test | Dev |
|---|---|---|---|
| AMQP host | `io.fiks.ks.no` | `io.fiks.test.ks.no` | `amqpHost` argument |
| AMQP port | 5671 (TLS) | 5671 (TLS) | 5671 (TLS) |
| API host | `api.fiks.ks.no` | `api.fiks.test.ks.no` | `apiHost` argument |
| API port | 443 (https) | 443 (https) | 443 (https) |
| Maskinporten audience | `https://maskinporten.no/` | `https://test.maskinporten.no/` | `https://test.maskinporten.no/` |
| Maskinporten token endpoint | `https://maskinporten.no/token` | `https://test.maskinporten.no/token` | `https://test.maskinporten.no/token` |

The Maskinporten token is renewed 10 seconds before it expires.

## AMQP

`WithAmqpConfiguration(applicationName, prefetchCount)` sets two values for the connection to the message queue:

| Setting | Default in the builder | Description |
|---|---|---|
| `applicationName` | Empty string | The name of the connection in RabbitMQ. Set it to something that identifies your application, so you and Fiks can find your connection in logs and in the RabbitMQ management view |
| `prefetchCount` | 10 | How many messages the client fetches before you have acknowledged them (AMQP QoS prefetch). A lower value spreads messages more evenly between several instances. A higher value gives more throughput |

## Rate limiting of connection attempts

The client limits how often it tries to open an AMQP connection, with a token bucket. The defaults are a bucket size of 5 and one new token every 2 seconds. When the bucket is empty, the client logs `Too many connection attempts. Rate limiting active.` and waits for a token.

The limit applies to one `FiksIOClient` instance. It does not limit how many clients you create, see [Operations](operations.md#use-one-long-lived-client).

The builder does not expose this setting. To change it, create the `AmqpConfiguration` yourself (see below) and pass a `RateLimitConfiguration`:

```csharp
var amqpConfiguration = new AmqpConfiguration(
    host: AmqpConfiguration.ProdHost,
    applicationName: "my-application",
    prefetchCount: 10,
    rateLimitConfiguration: new RateLimitConfiguration(bucketSize: 5, tokenRefillInterval: TimeSpan.FromSeconds(2)));
```

## Configure without the builder

### Convenience methods

`FiksIOConfiguration.CreateProdConfiguration` and `CreateTestConfiguration` build a configuration with an `X509Certificate2` for both Maskinporten and ASiC-E signing:

```csharp
var configuration = FiksIOConfiguration.CreateProdConfiguration(
    integrasjonId: integrasjonId,
    integrasjonPassord: integrasjonPassord,
    kontoId: kontoId,
    privatNokkel: kontoPrivateKeyPem,
    issuer: issuer,                                  // Maskinporten client ID
    maskinportenSertifikat: maskinportenCertificate,
    asiceSertifikat: asiceCertificate,               // must contain the private key
    applicationName: "my-application",               // optional
    keyIdentifier: null);                            // optional Maskinporten kid
```

`CreateTestConfiguration` takes the same values. Its first three parameters are named `fiksIntegrasjonId`, `fiksIntegrasjonPassord` and `fiksKontoId`.

### Every value yourself

Use the `FiksIOConfiguration` constructor if you need full control:

```csharp
var kontoConfiguration = new KontoConfiguration(kontoId, kontoPrivateKeyPem);

var integrasjonConfiguration = new IntegrasjonConfiguration(integrasjonId, integrasjonPassord);

var maskinportenConfiguration = new MaskinportenClientConfiguration(
    audience: "https://test.maskinporten.no/",
    tokenEndpoint: "https://test.maskinporten.no/token",
    issuer: issuer,
    numberOfSecondsLeftBeforeExpire: 10,
    certificate: maskinportenCertificate);

var apiConfiguration = new ApiConfiguration(scheme: "https", host: "api.fiks.test.ks.no", port: 443);

var amqpConfiguration = new AmqpConfiguration(
    host: AmqpConfiguration.TestHost,
    port: 5671,
    applicationName: "my-application",
    prefetchCount: 10);

var asiceSigningConfiguration = new AsiceSigningConfiguration(
    "/path/to/asice-public.pem",
    "/path/to/asice-private.pem");

var configuration = new FiksIOConfiguration(
    kontoConfiguration: kontoConfiguration,
    integrasjonConfiguration: integrasjonConfiguration,
    maskinportenConfiguration: maskinportenConfiguration,
    asiceSigningConfiguration: asiceSigningConfiguration,
    apiConfiguration: apiConfiguration,
    amqpConfiguration: amqpConfiguration);
```

If you leave out `apiConfiguration`, the API defaults to production. If you leave out `amqpConfiguration`, the AMQP host follows the API host: the production API gives `io.fiks.ks.no`, and the test API gives `io.fiks.test.ks.no`. For any other API host, the client cannot know the AMQP host, so pass `amqpConfiguration` yourself.

The namespaces are:

| Type | Namespace |
|---|---|
| `FiksIOConfiguration`, `FiksIOConfigurationBuilder`, `KontoConfiguration`, `AmqpConfiguration`, `RateLimitConfiguration` | `KS.Fiks.IO.Client.Configuration` |
| `ApiConfiguration`, `IntegrasjonConfiguration` | `KS.Fiks.IO.Send.Client.Configuration` |
| `MaskinportenClientConfiguration` | `Ks.Fiks.Maskinporten.Client` |
| `AsiceSigningConfiguration` | `KS.Fiks.IO.Crypto.Configuration` |

## Optional parameters to CreateAsync

`FiksIOClient.CreateAsync(configuration, ...)` takes these optional parameters:

| Parameter | Description |
|---|---|
| `loggerFactory` | An `ILoggerFactory` for logging from the client. See [Operations](operations.md#logging) |
| `httpClient` | An `HttpClient` the client uses for Maskinporten and the Fiks IO API |
| `publicKeyProvider` | Your own `IPublicKeyProvider`. See [Public key provider](keys-and-signing.md#public-key-provider) |

The other parameters are for testing.
