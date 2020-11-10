using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpReceiveConsumer : DefaultBasicConsumer, IAmqpReceiveConsumer
    {
        private const string DokumentlagerHeaderName = "dokumentlager-id";

        private readonly Guid _accountId;

        private readonly IAsicDecrypter _decrypter;

        private readonly IDokumentlagerHandler _dokumentlagerHandler;

        private readonly IFileWriter _fileWriter;

        private readonly ISendHandler _sendHandler;

        public AmqpReceiveConsumer(
            IModel model,
            IDokumentlagerHandler dokumentlagerHandler,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            Guid accountId)
            : base(model)
        {
            this._dokumentlagerHandler = dokumentlagerHandler;
            this._fileWriter = fileWriter;
            this._decrypter = decrypter;
            this._sendHandler = sendHandler;
            this._accountId = accountId;
        }

        public event EventHandler<MottattMeldingArgs> Received;

        public override void HandleBasicDeliver(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IBasicProperties properties,
            ReadOnlyMemory<byte> body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            if (Received == null)
            {
                return;
            }

            try
            {
                var receivedMessage = ParseMessage(properties, body);

                Received?.Invoke(
                    this,
                    new MottattMeldingArgs(
                        receivedMessage,
                        new SvarSender(
                            this._sendHandler,
                            receivedMessage,
                            new AmqpAcknowledgeManager(
                                () => Model.BasicAck(deliveryTag, false),
                                () => Model.BasicNack(deliveryTag, false, false),
                                () => Model.BasicNack(deliveryTag, false, true)))));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static bool IsDataInDokumentlager(IBasicProperties properties)
        {
            return ReceivedMessageParser.GetGuidFromHeader(properties.Headers, DokumentlagerHeaderName) != null;
        }

        private static Guid GetDokumentlagerId(IBasicProperties properties)
        {
            try
            {
                return ReceivedMessageParser.RequireGuidFromHeader(properties.Headers, DokumentlagerHeaderName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static bool HasPayload(IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            return IsDataInDokumentlager(properties) || body.Length > 0;
        }

        private MottattMelding ParseMessage(IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            var metadata = ReceivedMessageParser.Parse(this._accountId, properties);
            return new MottattMelding(
                HasPayload(properties, body),
                metadata,
                GetDataProvider(properties, body),
                this._decrypter,
                this._fileWriter);
        }

        private Func<Task<Stream>> GetDataProvider(IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (!HasPayload(properties, body))
            {
                return () => throw new FiksIOMissingDataException("No data in message");
            }

            if (IsDataInDokumentlager(properties))
            {
                return async () => await this._dokumentlagerHandler.Download(GetDokumentlagerId(properties));
            }

            return async () => await Task.FromResult(new MemoryStream(body.ToArray()));
        }
    }
}