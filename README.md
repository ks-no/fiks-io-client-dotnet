# fiks-io-client-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.client.svg)](https://www.nuget.org/packages/KS.Fiks.IO.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-io-client-dotnet/issues)

.net library compatible with _[.Net Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)_ for sending and receiving messages using [Fiks IO](//ks-no.github.io/fiks-platform/tjenester/fiksio/).

Fiks IO is a messaging system for the public sector in Norway. [About Fiks IO (Norwegian)](https://ks-no.github.io/fiks-platform/tjenester_under_utvikling/fiksio/)

## Installation
Install [KS.Fiks.IO.Client](https://www.nuget.org/packages/KS.Fiks.IO.Client) nuget package in your .net project.

## Prerequisites
To be able to use Fiks IO you have to have an active Fiks IO account with an associated integration. This can be setup for you organization at [FIKS-Konfigurasjon (prod)](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/) or [FIKS-Konfigurasjon (test)](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/).

## Examples
### Sending message
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below
meldingRequest = new MeldingRequest(
                            avsenderKontoId: senderId, // Sender id as Guid
                            mottakerKontoId: receiverId, // Receiver id as Guid
                            meldingType: messageType); // Message type as string
        
// Sending a file
await client.Send(meldingRequest, "c:\path\someFile.pdf");

// Sending a string
await client.Send(meldingRequest, "String to send", "string.txt");

// Sending a stream
await client.Send(meldingRequest, someStream, "stream.jpg");

// Sending message without payload
await client.Send(meldingRequest);
```

### Receiving message

#### Write zip to file

```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var onReceived = new EventHandler<MottattMeldingArgs>((sender, fileArgs) =>
                {
                    if(fileArgs.Melding.HasPayload) { // Verify that message has payload
                        fileArgs.Melding.WriteDecryptedZip("c:\path\receivedFile.zip");
                    }
                    fileArgs.SvarSender.Ack() // Ack message if write succeeded to remove it from the queue
                    
                });

client.NewSubscription(onReceived);
```

#### Process archive as stream
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var onReceived = new EventHandler<MottattMeldingArgs>((sender, fileArgs) =>
                {
                    if(fileArgs.Melding.HasPayload) { // Verify that message has payload
                        using (var archiveAsStream = fileArgs.Melding.DecryptedStream) 
                        {
                            // Process the stream
                        }
                    }
                    fileArgs.SvarSender.Ack() // Ack message if handling of stream succeeded to remove it from the queue
                });

client.NewSubscription(onReceived);
```

#### Reply to message
You can reply directly to a message using the ReplySender.
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var onReceived = new EventHandler<MottattMeldingArgs>((sender, fileArgs) =>
                {
                  // Process the message
                  
                  await fileArgs.SvarSender.Svar(/* message type */, /* message as string, path or stream */);
                  fileArgs.SvarSender.Ack() // Ack message to remove it from the queue
                });

client.NewSubscription(onReceived);
```
### Lookup
Using lookup, you can find which Fiks IO account to send a message to, given organization number, message type and access level needed to read the message.

```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var request = new LookupRequest(
    identifikator: "ORG_NO.987654321",
    meldingsprotokoll: "no.ks.test.fagsystem.v1",
    sikkerhetsniva: 4);

var receiverKontoId = await client.Lookup(request); 
```

### Configuration

Two convenience functions are provided for generating default configurations for *prod* and *test*,
`CreateMaskinportenProdConfig` and `CreateMaskinportenTestConfig`. Only the required configuration parameters must be provided,
the rest will be set to default values for the given environment.

```c#
// Prod config
var config = FiksIOConfiguration.CreateProdConfiguration(
    integrasjonId: integrationId,
    integrasjonPassord: integrationPassord,
    kontoId: kontoId,
    privatNokkel: privatNokkel,
    issuer: issuer,
    certificate: certificat
);

// Test config
var config = FiksIOConfiguration.CreateTestConfiguration(
    integrasjonId: integrationId,
    integrasjonPassord: integrationPassord,
    kontoId: kontoId,
    privatNokkel: privatNokkel,
    issuer: issuer,
    certificate: certificat
);
```

If necessary, all parameters of configuration can be set in detail.

```c#
// Fiks IO account configuration
var kontoConfig = new KontoConfiguration(
                    kontoId: /* Fiks IO accountId as Guid */,
                    privatNokkel: /* Private key, paired with the public key supplied to Fiks IO account */); 

// Id and password for integration associated to the Fiks IO account.
var integrasjonConfig = new IntegrasjonConfiguration(
                        integrasjonId: /* Integration id as Guid */,
                        integrasjonPassord: /* Integration password */);

// ID-porten machine to machine configuration
var maskinportenConfig = new MaskinportenClientConfiguration(
    audience: @"https://oidc-ver2.difi.no/idporten-oidc-provider/", // ID-porten audience path
    tokenEndpoint: @"https://oidc-ver2.difi.no/idporten-oidc-provider/token", // ID-porten token path
    issuer: @"oidc_ks_test",  // KS issuer name
    numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
    certificate: /* virksomhetssertifikat as a X509Certificate2  */);

// Optional: Use custom api host (i.e. for connecting to test api)
var apiConfig = new ApiConfiguration(
                scheme: "https",
                host: "api.fiks.test.ks.no",
                port: 443);

// Optional: Use custom amqp host (i.e. for connection to test queue)
var amqp = new AmqpConfiguration(
                host: "io.fiks.test.ks.no",
                port: 5671);

// Combine all configurations
var configuration = new FiksIOConfiguration(kontoConfig, integrationConfig, maskinportenConfig, apiConfig, amqpConfig);
```

### Public Key provider

By default when sending a message, the public key of the receiver will be fetched using the Catalog Api. If you instead 
need to provide the public key by some other means, you can implement the IPublicKeyProvider interface, and inject it 
when creating your client like this:
```c#
var client = new FiksIOClient(configuration, myPublicKeyProvider);
```

