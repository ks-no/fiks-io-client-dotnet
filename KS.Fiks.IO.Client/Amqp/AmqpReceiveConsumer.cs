using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using KS.Fiks.IO.Crypto.Asic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpReceiveConsumer : IAmqpReceiveConsumer
    {
        private const string DokumentlagerHeaderName = "dokumentlager-id";

        private readonly Guid _accountId;
        private readonly IAsicDecrypter _decrypter;
        private readonly IDokumentlagerHandler _dokumentlagerHandler;
        private readonly IFileWriter _fileWriter;
        private readonly ISendHandler _sendHandler;

        public AmqpReceiveConsumer(
            IChannel model,
            IDokumentlagerHandler dokumentlagerHandler,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            Guid accountId)
        {
            Channel = model;
            _dokumentlagerHandler = dokumentlagerHandler;
            _fileWriter = fileWriter;
            _decrypter = decrypter;
            _sendHandler = sendHandler;
            _accountId = accountId;
        }

        public IChannel Channel { get; }

        public event Func<MottattMeldingArgs, Task> ReceivedAsync;

        public event Func<ConsumerEventArgs, Task> ConsumerCancelledAsync;

        public async Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            if (ConsumerCancelledAsync != null)
            {
                await ConsumerCancelledAsync.Invoke(new ConsumerEventArgs(new[] { consumerTag })).ConfigureAwait(false);            }
        }

        public async Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Consumer {consumerTag} cancellation acknowledged.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Consumer {consumerTag} has started consuming messages.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task HandleBasicDeliverAsync(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IReadOnlyBasicProperties properties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            if (ReceivedAsync == null)
            {
                Console.WriteLine("No handler for received messages.");
                return;
            }

            try
            {
                var receivedMessage = ParseMessage(properties, body, redelivered);

                async Task AckMessageAsync()
                {
                    if (Channel != null)
                    {
                        await Channel.BasicAckAsync(deliveryTag, false, cancellationToken).ConfigureAwait(false);
                    }
                }

                async Task NackMessageAsync()
                {
                    if (Channel != null)
                    {
                        await Channel.BasicNackAsync(deliveryTag, false, false, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                async Task RequeueMessageAsync()
                {
                    if (Channel != null)
                    {
                        await Channel.BasicNackAsync(deliveryTag, false, true, cancellationToken).ConfigureAwait(false);
                    }
                }

                var acknowledgeManager = new AmqpAcknowledgeManager(
                    AckMessageAsync,
                    NackMessageAsync,
                    RequeueMessageAsync);

                var svarSender = new SvarSender(
                    _sendHandler,
                    receivedMessage,
                    acknowledgeManager);

                var mottattMeldingArgs = new MottattMeldingArgs(
                    receivedMessage,
                    svarSender);

                await ReceivedAsync.Invoke(mottattMeldingArgs).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex.Message}");
                if (Channel != null)
                {
                    await Channel.BasicNackAsync(deliveryTag, false, true, cancellationToken).ConfigureAwait(false);
                }

                throw;
            }
        }

        public async Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
        {
            Console.WriteLine($"Channel shutdown: {reason.Cause}");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        private MottattMelding ParseMessage(IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, bool resendt)
        {
            var metadata = ReceivedMessageParser.Parse(_accountId, properties, resendt);
            return new MottattMelding(
                HasPayload(properties, body),
                metadata,
                GetDataProvider(properties, body.ToArray()),
                _decrypter,
                _fileWriter);
        }

        private static bool HasPayload(IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            return IsDataInDokumentlager(properties) || body.Length > 0;
        }

        private static bool IsDataInDokumentlager(IReadOnlyBasicProperties properties)
        {
            return ReceivedMessageParser.GetGuidFromHeader(properties.Headers, DokumentlagerHeaderName) != null;
        }

        private Func<Task<Stream>> GetDataProvider(IReadOnlyBasicProperties properties, byte[] body)
        {
            if (!HasPayload(properties, body))
            {
                return () => throw new FiksIOMissingDataException("No data in message");
            }

            if (IsDataInDokumentlager(properties))
            {
                return async () => await _dokumentlagerHandler.Download(GetDokumentlagerId(properties)).ConfigureAwait(false);
            }

            return async () => await Task.FromResult<Stream>(new MemoryStream(body)).ConfigureAwait(false);
        }

        private static Guid GetDokumentlagerId(IReadOnlyBasicProperties properties)
        {
            return ReceivedMessageParser.RequireGuidFromHeader(properties.Headers, DokumentlagerHeaderName);
        }
    }
}