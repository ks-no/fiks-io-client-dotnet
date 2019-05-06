# fiks-io-client-dotnet
[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ks-no/fiks-io-client-dotnet/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/vpre/KS.fiks.io.client.svg)](https://img.shields.io/nuget/vpre/KS.fiks.io.client)
[![GitHub issues](https://img.shields.io/github/issues-raw/ks-no/kryptering-dotnet.svg)](//github.com/ks-no/fiks-io-client-dotnet/issues)

## Installation

## Prerequisites

## Examples

### Sending message

```c#
var client = new FiksIOClient(configuration); // See setup of configuration below
var messageRequest = new MessageRequest
        {
            MessageType = "ExampleMessageType",
            ReceiverAccountId = receiverId, // Receiver id as Guid
            SenderAccountId = senderId, // Sender id as Guid
            SvarPaMelding = svarPaMeldingId // SvarPaMelding id as Guid
        };
        
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
```c#
var client = new FiksIOClient(configuration); // See setup of configuration below

var request = new LookupRequest
{
    AccessLevel = 4,
    Identifier = "ORG_NO.987654321",
    MessageType = "ExampleMessageType"
};

var receiverAccount = await sut.Lookup(request);
```

### Configuration
```c#
// Fiks IO account configuration
var account = new AccountConfiguration(
                    accountId: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", // Fiks IO account id
                    publicKey: "xxx"); // Private key supplied to Fiks IO account

// Id and password for integration associated to the Fiks IO account.
var integration = new FiksIntegrationConfiguration(
                        integrationId: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
                        integrationPassword: "xxx");

// X509 certificate issued by ID-Porten
var certificate = new X509Certificate2(fileName: @"c:/path/certificate.pem", password: "xxx");

// ID-porten machine to machine configuration
var maskinporten = new MaskinportenClientConfiguration
{
    Audience = @"https://oidc-ver2.difi.no/idporten-oidc-provider/", // ID-porten audience path
    TokenEndpoint = @"https://oidc-ver2.difi.no/idporten-oidc-provider/token", // ID-porten token path
    Issuer = @"oidc_ks_test",  // KS issuer name
    NumberOfSecondsLeftBeforeExpire = 10, // The token will be refreshed 10 seconds before it expires
    Certificate = certificate
};

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

## API
