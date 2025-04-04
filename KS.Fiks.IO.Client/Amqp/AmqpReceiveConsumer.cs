using System;
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
        private readonly IAmqpWatcher _amqpWatcher;

        public AmqpReceiveConsumer(
            IChannel channel,
            IDokumentlagerHandler dokumentlagerHandler,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            IAmqpWatcher amqpWatcher,
            Guid accountId)
        {
            Channel = channel;
            _dokumentlagerHandler = dokumentlagerHandler;
            _fileWriter = fileWriter;
            _decrypter = decrypter;
            _sendHandler = sendHandler;
            _amqpWatcher = amqpWatcher;
            _accountId = accountId;
        }

        public IChannel Channel { get; }

        public event Func<MottattMeldingArgs, Task> ReceivedAsync;

        public event Func<ConsumerEventArgs, Task> ConsumerCancelledAsync;

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
                return;
            }

            var receivedMessage = ParseMessage(properties, body, redelivered);
            var acknowledgeManager = CreateAcknowledgeManager(deliveryTag, cancellationToken);
            var svarSender = new SvarSender(_sendHandler, receivedMessage, acknowledgeManager);

            var mottattMeldingArgs = new MottattMeldingArgs(receivedMessage, svarSender);
            await ReceivedAsync.Invoke(mottattMeldingArgs).ConfigureAwait(false);
        }

        public async Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
        {
            await _amqpWatcher.HandleChannelShutdown(channel, reason).ConfigureAwait(false);
        }

        public async Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            await _amqpWatcher.HandleBasicChannelCancel(consumerTag).ConfigureAwait(false);
        }

        public async Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            await _amqpWatcher.HandleBasicChannelCancelOk(consumerTag).ConfigureAwait(false);
        }

        public async Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            await _amqpWatcher.HandleBasicChannelConsumeOk(consumerTag).ConfigureAwait(false);
        }

        private AmqpAcknowledgeManager CreateAcknowledgeManager(ulong deliveryTag, CancellationToken cancellationToken)
        {
            return new AmqpAcknowledgeManager(
                () => AcknowledgeMessageAsync(deliveryTag, cancellationToken),
                () => RejectMessageAsync(deliveryTag, false, cancellationToken),
                () => RejectMessageAsync(deliveryTag, true, cancellationToken));
        }

        private async Task AcknowledgeMessageAsync(ulong deliveryTag, CancellationToken cancellationToken)
        {
            if (Channel?.IsOpen == true)
            {
                await Channel.BasicAckAsync(deliveryTag, false, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task RejectMessageAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken)
        {
            if (Channel?.IsOpen == true)
            {
                await Channel.BasicNackAsync(deliveryTag, false, requeue, cancellationToken).ConfigureAwait(false);
            }
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
                return async () =>
                    await _dokumentlagerHandler.Download(GetDokumentlagerId(properties)).ConfigureAwait(false);
            }

            return async () => await Task.FromResult<Stream>(new MemoryStream(body)).ConfigureAwait(false);
        }

        private static Guid GetDokumentlagerId(IReadOnlyBasicProperties properties)
        {
            return ReceivedMessageParser.RequireGuidFromHeader(properties.Headers, DokumentlagerHeaderName);
        }
    }
}