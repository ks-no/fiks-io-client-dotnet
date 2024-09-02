# Example Application for Fiks-IO-Client

## How it works

This application starts a simple console application that starts a subscription to Fiks-IO/Fiks-Protokoll messages as a BackgroundService in the [FiksIOSubscriber.cs](FiksIO/FiksIOSubscriber.cs) class.

It also listens to key-press to send a message to it self, through it's own Fiks-IO or Fiks-Protokoll account, with the [MessageSender.cs](FiksIO/MessageSender.cs) class. The message sent is a simple 'ping'-message that the [FiksIOSubcriber.cs](FiksIO/FiksIOSubscriber.cs) class replies with a 'pong'-message.

The application listens to different keys for different types of ping-messages. There is one for Fiks-IO and one for each protocol.

- Enter-key - Fiks-IO 'ping'-message
- A-key - Fiks-Arkiv protocol 'ping'-message
- P-key - Fiks-Plan protocol 'ping'-message
- M-key - Fiks-Matrikkelfoering protocol 'ping'-message
- L-key - Write log with: IsOpen(), Maskinporten reachable and result from the status of the account from Fiks-IO rest-services 

The L-key will write status of the _IsOpen()_ method can be used for health checking. The _IsOpen()_ shows the connection status based on the RabbitMQ heartbeat.
It writes also the status of "antallKonsumenter" (number of subscribers) from the Fiks IO API, that also can be used for health checking. The status property "antallKonsumenter" is the registered number of subscribers in RabbitMQ.

This is a very simple example of sending, receiving and replying to messages with this Fiks-IO-Client that logs information on the messages. It sends the `testfile.txt` file as payload, and prints the text inside the file when it receives the message.

If you're using a Fiks-Protokoll account, please remember to add yourself as an approved sending account. This is only necessary for Fiks-Protokoll accounts.

## Getting started

You will need a Fiks-Protokoll or a Fiks-IO account. See documentation on how at the [developers.fiks.ks.no](https://developers.fiks.ks.no/tjenester/fiksprotokoll/#hvordan-komme-i-gang-med-fiks-protokoll) portal.

## Configuration

The [appsettings.json](appsettings.json) file is for configuration of the Fiks-IO-client in this example application.
This file is then used by the [FikIOConfigurationBuilder.cs](FiksIO/FiksIOConfigurationBuilder.cs) class for setting up the client.

The [FikIOConfigurationBuilder.cs](FiksIO/FiksIOConfigurationBuilder.cs) in this example project uses the Fluent builder that is included in the Fiks-IO-Client. 
This is even more helpful with setting up a test or production client for you.

### appsettings.json
Here is the example appsetting.json file with hopefully some helpful comments:

```json
{
  "AppSettings": {
    "FiksIOConfig": {
        "ApiHost": "api.fiks.test.ks.no", // API host adress. Test: api.fiks.test.ks.no", Prod: "api.fiks.ks.no"
        "ApiPort": "443", // Port for the API host. 433 is correct
        "ApiScheme": "https", // Schema for the API. HTTPS is correct
        "AmqpHost": "io.fiks.test.ks.no", // AMQP host adress for message subscribe. Test: "io.fiks.test.ks.no", Prod: "io.fiks.ks.no" 
        "AmqpPort": "5671",  // AMQP host port for message subscribe. 5671 is correct
        "FiksIoAccountId": "<your account>", // Your Fiks Protokoll (Fiks-IO) account id
        "FiksIoIntegrationId": "<your integration id>", // Your Fiks Protokoll (Fiks-IO) integration id
        "FiksIoIntegrationPassword": "<your integration password>", // Your Fiks Protokoll (Fiks-IO) integration password
        "FiksIoIntegrationScope": "ks:fiks", // Leave this as it is
        "FiksIoPrivateKey": "<path to private key>", // Path to the private key that is paired with the public key uploaded to your Fiks Protokoll (Fiks-IO) account
        "MaskinPortenAudienceUrl": "https://test.maskinporten.no/", // Maskinporten Audience url. Test: "https://test.maskinporten.no", Prod: "https://maskinporten.no"
        "MaskinPortenCompanyCertificateThumbprint": "<your thumbprint>", // The thumbprint for the certificate used with Maskinporten
        "MaskinPortenCompanyCertificatePath": "", // The path to the certificate used with Maskinporten
        "MaskinPortenCompanyCertificatePassword": "", // The password for the certificate used with Maskinporten
        "MaskinPortenIssuer": "", // Maskinporten issuer
        "MaskinPortenTokenUrl": "https://test.maskinporten.no/token", // Token url for Maskinporten. Test: "https://test.maskinporten.no/token", Prod: "https://maskinporten.no/token" 
        "AsiceSigningPrivateKey": "", // The path to the private key for AsiceSigning of the payloads.
        "AsiceSigningPublicKey": "" // The path to the public key for verification of AsiceSigning of the payloads.
    }
  }
}
```

Here are some explanations:

#### FiksIoPrivateKey
This is the private key that is paired with the public key uploaded to your Fiks Protokoll (Fiks-IO) account.
It is not related to the Maskinporten certificate, but the private key part from a generated key pair. It is used for decrypting messages that is encrypted by the sender of messages with the public key uploaded to Fiks-Protokoll.

#### MaskinPortenXXX settings
Please look at the documentation at [developers.ks.fiks.no](https://developers.fiks.ks.no/felles/difiidportenklient/) for further explanations.

#### AsiceSigningPrivateKey
This is a path to a private key that you want to use for Asice signing. In this example application it must be a private key of a generated public/private key pair.
But you can also use a private key linked to the Maskinporten certificate if you like when setting up the Fiks-IO-Client. Then you must provide a single file containing the certificate and the privatekey and use a different method of the configuration builder in the Fiks-IO-Client.

#### AsiceSigningPublicKey
This is a path to a public key that you want to use for Asice signing verification. In this example application it must be a public key of a generated public/private key pair.


