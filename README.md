# fiks-io-client-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.client.svg)](https://img.shields.io/nuget/vpre/KS.fiks.io.client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-io-client-dotnet/issues)

.net core library for sending and receiving messages using Fiks IO.

Fiks IO is a messaging system for the public sector in Norway. [About Fiks IO (Norwegian)](https://ks-no.github.io/fiks-platform/tjenester_under_utvikling/svarinn/)

## Installation
Install _KS.fiks.io.client_ nuget package.

## Prerequisites
To be able to use Fiks IO you have to have an active Fiks IO account with an associated integration. This can be setup for you organization at [FIKS-Konfigurasjon (prod)](https://forvaltning.fiks.ks.no/fiks-konfigurasjon/) or [FIKS-Konfigurasjon (test)](https://forvaltning.fiks.test.ks.no/fiks-konfigurasjon/).

## Examples
### Sending message
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below
var messageRequest = new MessageRequest(
                            receiverAccountId: receiverId, // Receiver id as Guid
                            senderAccountId: senderId, // Sender id as Guid
                            messageType: messageType); // Message type as string
        
// Sending a file
await client.Send(messageRequest, "c:\path\someFile.pdf");

// Sending a string
await client.Send(messageRequest, "String to send", "string.txt");

// Sending a stream
await client.Send(messageRequest, someStream, "stream.jpg");
```

### Receiving message

#### Write zip to file

```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var onReceived = new EventHandler<MessageReceivedArgs>((sender, fileArgs) =>
                {
                    fileArgs.Message.WriteDecryptedZip("c:\path\receivedFile.zip");
                });

client.NewSubscription(onReceived);
```

#### Process archive as stream
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var onReceived = new EventHandler<MessageReceivedArgs>((sender, fileArgs) =>
                {
                    using (var archiveAsStream = fileArgs.Message.DecryptedStream) 
                    {
                        // Process the stream
                    }
                });

client.NewSubscription(onReceived);
```

### Lookup
Using lookup, you can find which Fiks IO account to send a message to, given organization number, message type and access level needed to read the message.

```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var request = new LookupRequest
{
    AccessLevel = 4,
    Identifier = "ORG_NO.987654321",
    MessageType = "ExampleMessageType"
};

var receiverAccount = await sut.Lookup(request); // Id for the account receiving the specified request
```

### Configuration
```c#
// Fiks IO account configuration
var account = new AccountConfiguration(
                    accountId: /* Fiks IO accountId as Guid */,
                    publicKey: /* Private key supplied to Fiks IO account */); 

// Id and password for integration associated to the Fiks IO account.
var integration = new FiksIntegrationConfiguration(
                        integrationId: /* Integration id as Guid */,
                        integrationPassword: /* Integration password */);

// ID-porten machine to machine configuration
var maskinporten = new MaskinportenClientConfiguration(
    audience: @"https://oidc-ver2.difi.no/idporten-oidc-provider/", // ID-porten audience path
    tokenEndpoint: @"https://oidc-ver2.difi.no/idporten-oidc-provider/token", // ID-porten token path
    issuer: @"oidc_ks_test",  // KS issuer name
    numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
    certificate: /* X509Certificate2 from ID-Porten */);

// Optional: Use custom api host (i.e. for connecting to test api)
var api = new FiksApiConfiguration(
                scheme: "https",
                host: "api.fiks.test.ks.no",
                port: 443);

// Optional: Use custom amqp host (i.e. for connection to test queue)
var amqp = new AmqpConfiguration(
                host: "io.fiks.test.ks.no",
                port: 5672);

// Combine all configurations
var configuration = new FiksIOConfiguration(account, integration, maskinporten, api, amqp);
```
