# fiks-io-client-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.client.svg)](https://www.nuget.org/packages/KS.Fiks.IO.Client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-io-client-dotnet/issues)


## About this library
This is a .NET library compatible with _[.Net Standard 2.1](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)_ for sending and receiving messages using [Fiks IO](//ks-no.github.io/fiks-platform/tjenester/fiksio/).

Fiks IO is a messaging system for the public sector in Norway. [About Fiks IO (Norwegian)](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/)

It is also the underlying messaging system for the **Fiks Protokoll** messages. Read more about Fiks Protokoll [here](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/)

### Integrity
The nuget package is signed with a KS certificate in our build process, stored securely in a safe build environment.
The package assemblies are also [strong-named](https://learn.microsoft.com/en-us/dotnet/standard/assembly/strong-named).

### Simplifying Fiks-IO
This client and its corresponding clients for other languages released by KS simplify the authentication, encryption of messages, and communication through Fiks-IO. 
For example Fiks-IO requires that certain [headers](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/#headere) are set in the messages. 
Using this client means that these details are hidden and simpflifies sending and receiving messages through Fiks-IO. You can read more about the Fiks-IO headers [here](https://ks-no.github.io/fiks-plattform/tjenester/fiksprotokoll/fiksio/#headere).

#### RabbitMQ
Fiks-IO is using RabbitMQ and this Fiks-IO-Client is using its client for connecting and receiving messages. Sending messages goes through the Fiks-IO Rest-API.
For more information on RabbitMQ, we recommend the documentation pages on connections from RabbitMQ [here](https://www.rabbitmq.com/connections.html).

## Installation
Install [KS.Fiks.IO.Client](https://www.nuget.org/packages/KS.Fiks.IO.Client) nuget package in your .net project.

## Prerequisites
To be able to use Fiks IO you have to have an active Fiks IO account with an associated integration. This can be setup for you organization at [FIKS-Konfigurasjon (prod)](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/) or [FIKS-Konfigurasjon (test)](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/).

## Usage recomendations
We recommend having a long-lived Fiks-IO-Client and connection to Fiks-IO. Creating a new Fiks-IO-Client on demand, meaning creating a new Fiks-IO-Client e.g. many times pr hour, is not recommended.
Connecting to Fiks-IO and RabbitMQ for subscription is costly and can hurt the RabbitMQ server through [high connection churn](https://www.rabbitmq.com/connections.html#high-connection-churn). 

We recommend reading through the RabbitMQ documentation on [connections](https://www.rabbitmq.com/connections.html) and [connections lifecycle](https://www.rabbitmq.com/connections.html#lifecycle).

### Health
The client also exposes the status of the connection to RabbitMQ through the [IsOpen()](#isopen) function. 
We recommend using this for monitoring the health of the client. 

### Logging
The Fiks-IO client can provide logging if you pass it a LoggingFactory. 
It is also highly recommended to either listen to RabbitMQ system EventLogs by your own means or use the RabbitMQEventLogger event listener util provided in the Fiks-IO client for converting them to log.

Please note that warnings and errors related to the RabbitMQ connection will only be visible through the RabbitMQ system EventLogs.
These are important events like:
- Connection is lost and the client will try [AutoRecovery](https://www.rabbitmq.com/dotnet-api-guide.html#recovery) 
- Succesfull [AutoRecovery](https://www.rabbitmq.com/dotnet-api-guide.html#recovery) is performed
- [AutoRecovery](https://www.rabbitmq.com/dotnet-api-guide.html#recovery) failed and connection could not be recovered.

Using the RabbitMQEventLogger util will show these events in your application log. 

See further down in this README for example on how to use the RabbitMQEventLogger util.

## Examples

### Example project
An example project is provided [here](ExampleApplication) in the `ExampleApplication` and the [Program.cs](ExampleApplication/Program.cs) program. 
This example program shows how to create a Fiks-IO-Client, subscribe, send and reply to messages.

The example project starts a console program that listens and subscribes to messages on your Fiks-IO account. 
It listens to the Enter key in the console, that then triggers it to send a 'ping' message to your Fiks-IO account.
When the program receives the 'ping' message it will reply to that message with a 'pong' message. 

The program also shows the following features:
- It uses the optional `klientMeldingId` when sending messages 
- It uses the optional `klientKorrelasjonsId` when sending messages, and shows that it is returned in the response from the receiver of the first message 
- It utilizes the `IsOpen()` feature to show the connection-status.



Read more in the [README.md](ExampleApplication/README.md) file for the example application.

### Sending message
```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below
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

```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below

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
```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below

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
```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below

var onReceived = new EventHandler<MottattMeldingArgs>((sender, fileArgs) =>
                {
                  // Process the message
                  
                  await fileArgs.SvarSender.Svar(/* message type */, /* message as string, path or stream */);
                  fileArgs.SvarSender.Ack() // Ack message to remove it from the queue
                });

client.NewSubscription(onReceived);
```
### Lookup
Using lookup, you can find which Fiks IO account to send a message to, given an organization number, message type and access level needed to read the message.

```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below

var request = new LookupRequest(
    identifikator: "ORG_NO.987654321",
    meldingsprotokoll: "no.ks.test.fagsystem.v1",
    sikkerhetsniva: 4);

var receiverKontoId = await client.Lookup(request); 
```

### GetKonto
Using GetKonto, you can get information about a Fiks IO account, e.g. municipality number and organization name, given the account id.

```csharp
var client = await FiksIOClient.CreateAsync(configuration); // See setup of configuration below

var onReceived = new EventHandler<MottattMeldingArgs>(async (sender, fileArgs) =>
                {
                    var konto = await client.GetKonto(fileArgs.Melding.AvsenderKontoId); // Get information about the sender account
                    ...
                });

client.NewSubscription(onReceived);
```


### IsOpen
This method can be used to check if the amqp connection is open.

### Configuration

The fluent configuration builder FiksIOConfigurationBuilder can be used for creating the FiksIOConfiguration, where you build either for *test* or *prod*.  
Only the required configuration parameters must be provided when you use these two and the rest will be set to default values for the given environment.

You can also create the configuration yourself where also two convenience functions are provided for generating default configurations for *prod* and *test*,
`CreateMaskinportenProdConfig` and `CreateMaskinportenTestConfig`. Also here will only the required configuration parameters are needed.

#### Logging from the Fiks-IO client
Logging is available by providing the Fiks-IO-Client with a ILoggerFactory. Example of this is provided in the ExampleApplication project.

#### Logging from the RabbitMQ client
The Fiks-IO-Client uses the official [RabbitMQ-Client for .NET](https://github.com/rabbitmq/rabbitmq-dotnet-client). This client logs to system EventLog with the eventsource name "rabbitmq-dotnet-client". 
We have created a RabbitMQEventLogger util-class for easy logging of these events to your logs.
This util-class will have to be initiated in your program once. Take a look at the following example or in the Program.cs of the ExampleApplication project:

```csharp
_rabbitMqEventLogger = new RabbitMQEventLogger(loggerFactory, EventLevel.Informational);
```

#### Create with builder examples:

```csharp

// Prod alternative 1
// Prod config with public/private key for asice signing
var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("My unique name for this application", 1) // Optional but recomended: default values will be a generated application name, 10 prefetch count, and keepAlive = false
                .WithMaskinportenConfiguration(certificate, issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(kontoId, privateKey) // privateKey is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account
                .WithAsiceSigningConfiguration(asicePublicKeyFilepath, asicePrivateKeyPath) // A public/private key pair to sign the asice packages
                .BuildProdConfiguration();
                
                
// Prod alternative 2
// Prod config with a X509Certificate2 certificate for asice signing
var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("My unique name for this application", 1) // Optional but recomended: default values will be a generated application name, 10 prefetch count, and keepAlive = false
                .WithMaskinportenConfiguration(certificate, issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(kontoId, privateKey) // privateKey is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account
                .WithAsiceSigningConfiguration(certificate2) // A X509Certificate2 certificate that also contains the private key
                .BuildProdConfiguration();

// Test alternative 1
// Test config with public/private key for asice signing
var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("My unique name for this application", 1) // Optional but recomended: default values will be a generated applicationname, 10 prefetch count, and keepAlive = false
                .WithMaskinportenConfiguration(certificate, issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(kontoId, privateKey) // privateKey is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account
                .WithAsiceSigningConfiguration(asicePublicKeyFilepath, asicePrivateKeyPath) //  A public/private key pair to sign the asice packages
                .BuildTestConfiguration();
            
// Test alternative 2
// Test config with a X509Certificate2 certificate for asice signing
var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("My unique name for this application", 1) // Optional but recomended: default values will be a generated applicationname, 10 prefetch count, and keepAlive = false
                .WithMaskinportenConfiguration(certificate, issuer)
                .WithFiksIntegrasjonConfiguration(integrationId, integrationPassword)
                .WithFiksKontoConfiguration(kontoId, privateKey) // privateKey is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account
                .WithAsiceSigningConfiguration(certificate2) // A X509Certificate2 certificate that also contains the private key
                .BuildTestConfiguration();
                
);
```

Explanations:

`.WithFiksKontoConfiguration(kontoId, privateKey)`: The **privateKey** parameter is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account. The public key uploaded to your account is the public key that other clients will use for encrypting messages when sending messages to your account. The **privateKey** is then used for decrypting messages.   


#### Create without builder example:
Here are examples using the two convenience methods.

```csharp
// Prod config with a asice X509Certificate2 certificate
var config = FiksIOConfiguration.CreateProdConfiguration(
    integrasjonId: integrationId,
    integrasjonPassord: integrationPassord,
    kontoId: kontoId,
    privatNokkel: privatNokkel,
    issuer: issuer, // klientid for maskinporten
    maskinportenSertifikat: maskinportenSertifikat, // The certificate used with maskinporten
    asiceSertifikat: asiceSertifikat, // A X509Certificate2 certificate that also contains the corresponding private-key
    keepAlive: false, // Optional: use this if you want to use the keepAlive functionality. Default = false
    applicationName: null // Optional: use this if you want your client's activity to have a unique name in logs.
);

// Test config
var config = FiksIOConfiguration.CreateTestConfiguration(
    integrasjonId: integrationId,
    integrasjonPassord: integrationPassord,
    kontoId: kontoId,
    privatNokkel: privatNokkel, 
    issuer: issuer, // klientid for maskinporten
    maskinportenSertifikat: maskinportenSertifikat, // The certificate used with maskinporten as a X509Certificate2
    asiceSertifikat: asiceSertifikat, // A X509Certificate2 certificate that also contains the corresponding private-key
    keepAlive: false, // Optional: use this if you want to use the keepAlive functionality. Default = false
    applicationName: null // Optional: use this if you want your client's activity to have a unique name in logs.
);
```
If necessary, all parameters of configuration can be set in detail.

```csharp
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
    audience: @"https://test.maskinporten.no/", // Maskinporten audience path
    tokenEndpoint: @"https://test.maskinporten.no/token", // Maskinporten token path
    issuer: @"issuer",  // Issuer name
    numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
    certificate: /* virksomhetssertifikat as a X509Certificate2  */);

// Optional: Use custom api host (i.e. for connecting to test api)
var apiConfig = new ApiConfiguration(
                scheme: "https",
                host: "api.fiks.test.ks.no",
                port: 443);

// Optional: Use custom amqp host (i.e. for connection to test queue). 
// Optional: Set keepAlive: true if you want the FiksIOClient to check if amqp connection is open every 5 minutes and automatically reconnect. 
// another option to using keepAlive is to use the isOpen() method on the FiksIOClient and implement a keepalive strategy yourself 
var amqp = new AmqpConfiguration(
                host: "io.fiks.test.ks.no",
                port: 5671,
                applicationName: "my-application",
                keepAlive: false); 

// Configuration for Asice signing. Not optional. Either use a public/private key-pair or a X509Certificate2 that contains the corresponding private key.
var asiceSigning = new AsiceSigningConfiguration(
                publicKeyPath: "/path/to/file",
                privateKeyPath: "/path/to/file"); 

// Combine all configurations
var configuration = new FiksIOConfiguration(
                        kontoConfiguration: kontoConfig, 
                        integrasjonConfiguration: integrationConfig, 
                        maskinportenConfiguration: maskinportenConfig, 
                        apiConfiguration: apiConfig,  // Optional
                        amqpConfiguration: amqpConfig, // Optional
                        asiceSigningConfiguration: asiceSigning); 
```

#### Configuration setting details:

##### Ampq:
- **keepAlive**: Optional setting. Set the `keepAlive` to true if you want the client to check every 5 minutes if the amqp connection is open and automatically reconnect
- **applicationName**: Optional but recomended. Gives the Fiks-IO queue a name that you provide. Makes it easier to identify which queue is yours from a logging and management perspective.

#### Fiks-IO Konto: 
- **privatNokkel**: The `privatNokkel` property expects a private key in PKCS#8 format. Private key which has a PKCS#1 will cause an exception. This is the private key associated with the public key uploaded to your fiks-io/fiks-protokoll account.
It is not required to be derived from the Maskinporten certificate. See example on how to convert a PKCS#1 or generate private/public keys further down.

#### Asice signing:
Asice signing is required since version 3.0.0 of this client. More information on Asice signing can be found [here](https://docs.digdir.no/dpi_dokumentpakke_sikkerhet.html).

There are two ways of setting this up, either with a public/private key pair or a x509Certificate2 that also holds the private key.
If you are reusing the x509Certificate2 from the `maskinporten` configuration you might have to inject the corresponding private key.

A PKCS#1 key can be converted using this command:
```powershell
openssl pkcs8 -topk8 -nocrypt -in <pkcs#1 key file> -out <pkcs#8 key file>
```

Example for generating private/public key pair for signing
```powershell
openssl genrsa -out key.pem 4096

openssl req -new -x509 -key key.pem -out public.pem -days 99999
```

**Examples:**

x509Certificate2 with a private key `AsiceSigningConfiguration(X509Certificate2 x509Certificate2);`

Path to public/private key: `AsiceSigningConfiguration(string publicCertPath, string privateKeyPath);`

### Public Key provider

By default when sending a message, the public key of the receiver will be fetched using the Catalog Api. If you instead 
need to provide the public key by some other means, you can implement the IPublicKeyProvider interface, and inject it 
when creating your client like this:
```csharp
var client = new FiksIOClient(configuration, myPublicKeyProvider);
```

