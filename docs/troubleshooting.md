# Troubleshooting

Common errors, what causes them, and how to fix them. Pass an `ILoggerFactory` to `CreateAsync` and set up [`RabbitMQEventLogger`](operations.md#logging) first: many problems are only visible in the log.

## The configuration does not build

**`ArgumentException: ... missing. Have you called the With...( ... ) in this builder?`**

A required part of the configuration is missing. Call the builder method named in the message. Maskinporten, integration, account and ASiC-E signing are all required. See [Configuration](configuration.md#builder-methods).

**`ArgumentException: MaskinportenConfiguration has both a certificate and a key pair`**

You called both `WithMaskinportenConfiguration` and `WithMaskinportenKeyPairConfiguration`. Use one of them.

**`ArgumentException: keyIdentifier missing`**

`WithMaskinportenKeyPairConfiguration` requires the `kid` of the key registered on your Maskinporten client. See [Maskinporten](maskinporten.md#rsa-key-pair).

## The client does not start

**`InvalidOperationException: No configured private key can decrypt messages for account ...`**

The public key in the Fiks IO catalog does not match any of the private keys you configured, and automatic public key upload is enabled. Check that you use the right private key for the account, or replace the public key in the catalog. See [Automatic public key sync](automatic-public-key-sync.md).

**`InvalidOperationException: Configured public key for account ... does not match any configured private key`**

The public certificate you passed to `WithFiksKontoConfiguration` does not belong to any of the private keys. Check that the files are from the same key pair.

**`Failed to read public key from catalog for account ...; cannot safely synchronize. Aborting startup.`**

Automatic public key upload is enabled and the Fiks IO catalog was unavailable. The client does not upload a key when it does not know what is registered. Try again later. See [Scenario 6](automatic-public-key-sync.md#scenario-6--catalog-unavailable-at-startup-upload-feature-enabled).

**`FiksIOAmqpConnectionFailedException: Failed to create connection to ...`**

The client could not connect to RabbitMQ. The inner exception has the cause. Check that:

- the Maskinporten configuration is correct and the client can get a token (see `FiksIOMaskinportenTokenException` below)
- the integration has access to the account in FIKS-Konfigurasjon
- you use the right environment: test credentials do not work in production, and the other way round
- your network allows outgoing traffic to the AMQP host on port 5671

**`FiksIOMaskinportenTokenException: Failed to retrieve Maskinporten token.`**

The client could not get a token from Maskinporten. Check the `issuer`, the certificate or key pair, the `kid`, and that the Maskinporten client has access to the `ks:fiks` scope.

## Messages are not decrypted

**The log says `No configured private key can decrypt messages for account ...` at startup, but the client starts**

The public key in the catalog does not match any configured private key, and automatic public key upload is disabled. The client starts, but incoming messages will fail to decrypt. Configure the matching private key, or register the right public key on the account.

**Decryption fails after a key rotation**

Messages already on the queue are encrypted with the old public key. Keep the old private key in the configuration until the queue is empty. See [Key rotation](automatic-public-key-sync.md#key-rotation-step-by-step-guide).

**The private key is in the wrong format**

The client expects the account key in PKCS#8 format (`-----BEGIN PRIVATE KEY-----`), as PEM text, not a file path. See [Keys and signing](keys-and-signing.md#configure-the-private-key) for how to convert a PKCS#1 key.

## Token renewal fails after a while

**`ObjectDisposedException` when the client renews the Maskinporten token**

You use `WithMaskinportenKeyPairConfiguration` and the `RSA` object has been disposed, for example by a `using` statement. Keep it alive as long as the client is in use. See [Maskinporten](maskinporten.md#3-configure-the-client).

## Messages stop arriving

- Check `IsOpenAsync()` and the log for connection events. See [Operations](operations.md#health-checks).
- Check that your handler acknowledges every message. Unacknowledged messages fill up the `prefetchCount` slots, and the client receives no more messages until they are acknowledged. See [Acknowledge messages](sending-and-receiving.md#acknowledge-messages).
- Check whether an exception in your handler stops it before it acknowledges the message.

## Testing API calls by hand

To call the Fiks IO API with `curl`, you need a Maskinporten token. The example application can fetch one for you, see [ExampleApplication](../ExampleApplication/README.md#maskinporten-token-and-api-calls). A Maskinporten token is valid for 2 minutes.

## Building the library

For build errors such as `OpenSslCryptographicException: ... invalid digest` on Fedora or RHEL, see [CONTRIBUTING.md](../CONTRIBUTING.md#strong-name-signing).
