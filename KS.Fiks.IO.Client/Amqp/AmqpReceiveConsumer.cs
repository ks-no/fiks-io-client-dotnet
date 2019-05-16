using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpReceiveConsumer : DefaultBasicConsumer, IAmqpReceiveConsumer
    {
        private const string DokumentlagerHeaderName = "DOKUMENTLAGER_PAYLOAD";

        private readonly IAsicDecrypter _decrypter;

        private readonly IFileWriter _fileWriter;

        private readonly ISendHandler _sendHandler;

        private readonly IDokumentlagerHandler _dokumentlagerHandler;

        private readonly Guid _accountId;

        public AmqpReceiveConsumer(
            IModel model,
            IDokumentlagerHandler dokumentlagerHandler,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            Guid accountId)
            : base(model)
        {
            _dokumentlagerHandler = dokumentlagerHandler;
            _fileWriter = fileWriter;
            _decrypter = decrypter;
            _sendHandler = sendHandler;
            _accountId = accountId;
        }

        public event EventHandler<MessageReceivedArgs> Received;

        public override void HandleBasicDeliver(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IBasicProperties properties,
            byte[] body)
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
                    new MessageReceivedArgs(receivedMessage, new ReplySender(_sendHandler, receivedMessage, () => Model.BasicAck(deliveryTag, false))));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private ReceivedMessage ParseMessage(IBasicProperties properties, byte[] body)
        {
            var metadata = ReceivedMessageParser.Parse(_accountId, properties);
            return new ReceivedMessage(metadata, GetDataProvider(properties, body, metadata.MessageId), _decrypter, _fileWriter);
        }


        private Func<Task<Stream>> GetDataProvider(IBasicProperties properties, byte[] body, Guid messageId)
        {
            if (IsDataInDokumentlager(properties))
            {
                return async () => await _dokumentlagerHandler.Download(messageId);
            }
            else
            {
                return async () => await Task.FromResult(new MemoryStream(body));
            }
        }

        private bool IsDataInDokumentlager(IBasicProperties properties)
        {
            return properties.Headers.ContainsKey(DokumentlagerHeaderName);
        }


        private Func<Stream> GetDataProvider(IBasicProperties properties, byte[] body, Guid messageId)
        {
            if (IsDataInDokumentlager(properties))
            {
                return () => _dokumentlagerHandler.Download(messageId);
            }
            else
            {
                return () => new MemoryStream(body);
            }
        }

        private bool IsDataInDokumentlager(IBasicProperties properties)
        {
            return properties.Headers.ContainsKey(DokumentlagerHeaderName);
        }
    }
}