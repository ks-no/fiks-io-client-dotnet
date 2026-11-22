# Keys and signing

How to create and configure the account key and the ASiC-E signing key. For the Maskinporten key, see [Maskinporten](maskinporten.md). For an overview of all three keys, see [Concepts](concepts.md#the-three-keys).

## The account key

Senders encrypt messages to your account with the public key registered for the account in the Fiks IO catalog. Your client decrypts them with the matching private key. This key pair belongs to the account and is not related to your Maskinporten certificate.

### Create a key pair

```bash
# Private key in PKCS#8 format
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:4096 -out konto-private.pem

# Public key as a self-signed X.509 certificate
openssl req -new -x509 -key konto-private.pem -out konto-public.pem -days 99999
```

Then register the public certificate on the account. You can either:

- upload `konto-public.pem` to the account in FIKS-Konfigurasjon ([prod](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/), [test](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/)), or
- let the client upload it when it starts, see [Automatic public key sync](automatic-public-key-sync.md).

### Configure the private key

Pass the private key as PEM text:

```csharp
.WithFiksKontoConfiguration(kontoId, File.ReadAllText("konto-private.pem"))
```

The client expects PKCS#8, which starts with `-----BEGIN PRIVATE KEY-----`. If your key starts with `-----BEGIN RSA PRIVATE KEY-----`, it is PKCS#1. Convert it:

```bash
openssl pkcs8 -topk8 -nocrypt -in pkcs1-key.pem -out pkcs8-key.pem
```

To rotate the account key without losing messages, configure several private keys. The client tries each of them when it decrypts. See [Key rotation](automatic-public-key-sync.md#key-rotation-step-by-step-guide).

## ASiC-E signing

The payload of each message you send is packed in an [ASiC-E](https://docs.digdir.no/dpi_dokumentpakke_sikkerhet.html) container and signed. Signing is required. There are two ways to set it up:

| Option | Builder method |
|---|---|
| A key pair in two files | `WithAsiceSigningConfiguration(publicCertPath, privateKeyPath)` |
| An `X509Certificate2` that contains the private key | `WithAsiceSigningConfiguration(certificate)` |

### Key pair

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:4096 -out asice-private.pem
openssl req -new -x509 -key asice-private.pem -out asice-public.pem -days 99999
```

```csharp
.WithAsiceSigningConfiguration("/path/to/asice-public.pem", "/path/to/asice-private.pem")
```

This method takes **file paths**, not PEM text.

### Certificate

```csharp
.WithAsiceSigningConfiguration(asiceCertificate)
```

The certificate must contain the private key. You can use your business certificate, the same as for Maskinporten. If you load the certificate in a way that leaves out the private key, for example from a `.cer` file, you must attach the private key to it.

## Public key provider

When you send a message, the client gets the receiver's public key from the Fiks IO catalog. To get it some other way, implement `IPublicKeyProvider` from the namespace `KS.Fiks.IO.Send.Client.Catalog`:

```csharp
public class MyPublicKeyProvider : IPublicKeyProvider
{
    public Task<Org.BouncyCastle.X509.X509Certificate> GetPublicKey(Guid receiverAccountId)
    {
        // Return the receiver's public certificate, for example from your own store
        throw new NotImplementedException();
    }
}
```

Pass it to `CreateAsync`:

```csharp
var client = await FiksIOClient.CreateAsync(configuration, publicKeyProvider: new MyPublicKeyProvider());
```
