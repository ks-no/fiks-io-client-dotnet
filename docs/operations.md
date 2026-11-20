# Running in production

How to run the client in a service: client lifetime, health checks, logging and shutdown.

## Use one long-lived client

Create one `FiksIOClient` when your application starts, and keep it until the application stops. Do not create a new client for each message or each request.

Each client opens a connection to RabbitMQ and fetches a Maskinporten token. Opening connections often is costly and can harm the RabbitMQ server through [high connection churn](https://www.rabbitmq.com/connections.html#high-connection-churn). Read more about [connections](https://www.rabbitmq.com/connections.html) and [their lifecycle](https://www.rabbitmq.com/connections.html#lifecycle) in the RabbitMQ documentation.

With dependency injection, register the client as a singleton:

```csharp
var client = await FiksIOClient.CreateAsync(configuration, loggerFactory);
services.AddSingleton<IFiksIOClient>(client);
```

The client uses the RabbitMQ client's automatic recovery: if the connection drops, it reconnects and resumes the subscription by itself.

## Health checks

| Check | What it tells you |
|---|---|
| `await client.IsOpenAsync()` | Whether this client's AMQP connection and channel are open |
| `(await client.GetKontoStatus(kontoId)).AntallKonsumenter` | How many consumers RabbitMQ has registered on the account's queue, across all your instances |
| `client.IsAutomaticPublicKeyUploadEnabled` | Whether this client uploads the account's public key on startup, see [Automatic public key sync](automatic-public-key-sync.md) |
| `await client.ValidatePublicKeyAgainstPrivateKeyAsync()` | Whether a configured private key matches the public key in the catalog. Returns `false` if no key is registered |

`IsOpenAsync` is cheap and suitable for a liveness check. `GetKontoStatus` and `ValidatePublicKeyAgainstPrivateKeyAsync` call the Fiks IO API.

## Logging

### Logging from the client

Pass an `ILoggerFactory` to `CreateAsync`:

```csharp
var client = await FiksIOClient.CreateAsync(configuration, loggerFactory);
```

The client then logs startup, key validation and connection events such as connection shutdown, blocked connections, and recovery that succeeded or failed.

### Logging from the RabbitMQ client

The RabbitMQ .NET client writes its own internal events, including details about exceptions during recovery, to an `EventSource` named `rabbitmq-client`. These do not go through your `ILoggerFactory` unless you listen for them. `RabbitMQEventLogger` forwards them to your log:

```csharp
// Keep a reference for the lifetime of the application
var rabbitMqEventLogger = new RabbitMQEventLogger(loggerFactory, EventLevel.Informational);
```

`RabbitMQEventLogger` is in the namespace `KS.Fiks.IO.Client.Amqp.RabbitMQ`, and `EventLevel` in `System.Diagnostics.Tracing`. Create it once.

## Shut down

`FiksIOClient` implements `IAsyncDisposable`. Dispose it when the application stops, so the client closes the channel and connection to RabbitMQ:

```csharp
await client.DisposeAsync();
```

Messages that your handler has not acknowledged when the connection closes, are put back on the queue by RabbitMQ and delivered again.
