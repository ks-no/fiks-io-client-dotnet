# Sending and receiving messages

How to send a message, receive and reply to messages, and acknowledge them. The examples assume you have a `client` created with `FiksIOClient.CreateAsync`, see [Configuration](configuration.md).

## Send a message

Create a `MeldingRequest` and pass it to `Send` together with the payload:

```csharp
var request = new MeldingRequest(
    avsenderKontoId: myKontoId,       // your account
    mottakerKontoId: receiverKontoId, // the receiver's account
    meldingType: "no.ks.fiks.arkiv.v1.ping");

// A file on disk
await client.Send(request, "/path/to/file.pdf");

// A string, sent as a file with the given name
await client.Send(request, "Text to send", "message.txt");

// A stream, sent as a file with the given name
await client.Send(request, stream, "image.jpg");

// No payload
await client.Send(request);
```

To send several files in one message, pass a list of payloads:

```csharp
var payloads = new List<IPayload>
{
    new FilePayload("/path/to/file.pdf"),
    new StringPayload("<xml/>", "metadata.xml"),
};
await client.Send(request, payloads);
```

Every `Send` overload takes an optional `CancellationToken` as the last argument.

`Send` returns a `SendtMelding` with the `MeldingId` that Fiks IO assigned. The client looks up the receiver's public key in the Fiks IO catalog. If you need to get the key some other way, see [Public key provider](keys-and-signing.md#public-key-provider).

### MeldingRequest parameters

| Parameter | Required | Description |
|---|---|---|
| `avsenderKontoId` | Yes | Your account ID |
| `mottakerKontoId` | Yes | The receiver's account ID |
| `meldingType` | Yes | The message type, for example a Fiks Protokoll message type |
| `ttl` | No | How long the message lives on the receiver's queue. Default 2 days |
| `headere` | No | Extra headers as a `Dictionary<string, string>` |
| `svarPaMelding` | No | The `MeldingId` this message replies to. Set automatically when you use `SvarSender` |
| `klientMeldingId` | No | Your own ID for the message, see [below](#your-own-ids-klientmeldingid-and-klientkorrelasjonsid) |
| `klientKorrelasjonsId` | No | Your own correlation ID, see [below](#your-own-ids-klientmeldingid-and-klientkorrelasjonsid) |

## Receive messages

Register a handler with `NewSubscriptionAsync`. The client calls it for each message on your account's queue:

```csharp
await client.NewSubscriptionAsync(async args =>
{
    var melding = args.Melding;

    if (melding.HasPayload)
    {
        await melding.WriteDecryptedZip("/path/to/received.zip");
    }

    await args.SvarSender.AckAsync(); // remove the message from the queue
});
```

`NewSubscriptionAsync` takes an optional second handler, `onCanceled`, which is called if RabbitMQ cancels the subscription.

### Read the payload

The payload is an ASiC-E container (a zip file). You can read it in three ways:

| Member | Gives you |
|---|---|
| `WriteDecryptedZip(path)` | Writes the decrypted container to a file |
| `DecryptedStream` | The decrypted container as a stream |
| `DecryptedPayloads` | The files inside the container, as `IPayload` objects |

```csharp
await using var zip = await args.Melding.DecryptedStream;
// process the zip stream
```

Check `HasPayload` first. A message without a payload throws `FiksIOMissingDataException` if you try to read it.

### Message properties

`args.Melding` also gives you `MeldingId`, `MeldingType`, `AvsenderKontoId`, `MottakerKontoId`, `SvarPaMelding`, `KlientMeldingId`, `KlientKorrelasjonsId`, `Headere`, `Ttl` and `Resendt` (true if the message has been delivered before).

## Reply to a message

Use `SvarSender.Svar` in the handler. The reply goes to the sender of the original message, with `svarPaMelding` and `klientKorrelasjonsId` copied from it:

```csharp
await client.NewSubscriptionAsync(async args =>
{
    await args.SvarSender.Svar("no.ks.fiks.arkiv.v1.pong", "Reply text", "reply.txt");
    await args.SvarSender.AckAsync();
});
```

`Svar` has the same payload variants as `Send`: a file path, a string with a file name, a stream with a file name, a list of payloads, or no payload. Each overload takes an optional `klientMeldingId`.

## Acknowledge messages

Every message must be acknowledged. Until your handler does, the message stays on the queue as unacknowledged and takes up one of the `prefetchCount` slots, see [Configuration](configuration.md#amqp). `SvarSender` gives you three choices:

| Method | Use when | What happens to the message |
|---|---|---|
| `AckAsync()` | The message is handled | Removed from the queue |
| `NackAsync()` | The message can never be handled | Moved to the dead letter queue. No error message is sent to the sender |
| `RejectWithRequeueAsync()` | The message cannot be handled now, but may succeed later | Put back on the queue with the delivery count increased. When the maximum delivery count is reached, it is moved to the dead letter queue and an error message is sent to the sender |

```csharp
await args.SvarSender.AckAsync();
await args.SvarSender.NackAsync();
await args.SvarSender.RejectWithRequeueAsync();
```

`NackWithRequeueAsync()` is deprecated and now behaves like `RejectWithRequeueAsync()`. Use `RejectWithRequeueAsync()`.

## Your own IDs: klientMeldingId and klientKorrelasjonsId

Fiks IO assigns the `MeldingId`. Two optional fields let you add your own:

- **`klientMeldingId`** (GUID): your ID for one message. Set it on `MeldingRequest` or as the last argument to `Svar`.
- **`klientKorrelasjonsId`** (string): an ID that follows a conversation. Set it on the first `MeldingRequest`. When the receiver replies with `SvarSender.Svar`, the reply gets the same `klientKorrelasjonsId`.

Both are sent as headers, and the receiver can read them on `args.Melding`.

```csharp
var request = new MeldingRequest(
    avsenderKontoId: myKontoId,
    mottakerKontoId: receiverKontoId,
    meldingType: "ping",
    klientMeldingId: Guid.NewGuid(),
    klientKorrelasjonsId: "order-12345");
```

## Look up an account

`GetKonto` returns information about an account, such as the organisation name (`FiksOrgNavn`), organisation number and municipality number:

```csharp
var konto = await client.GetKonto(args.Melding.AvsenderKontoId);
Console.WriteLine($"Message from {konto.FiksOrgNavn} ({konto.KontoNavn})");
```

`GetKontoStatus` returns the status of an account, including the number of consumers (`AntallKonsumenter`) and unread messages (`AntallUavhentedeMeldinger`). See [Operations](operations.md#health-checks).
