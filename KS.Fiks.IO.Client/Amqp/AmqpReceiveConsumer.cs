using System;
using System.IO;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpReceiveConsumer : DefaultBasicConsumer, IAmqpReceiveConsumer
    {
        private readonly IAsicDecrypter _decrypter;

        private readonly IFileWriter _fileWriter;

        private readonly ISendHandler _sendHandler;

        private readonly Guid _accountId;

        public AmqpReceiveConsumer(
            IModel model,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            Guid accountId)
            : base(model)
        {
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
            var receivedMessage = ParseMessage(properties, body);

            if (Received == null)
            {
                return;
            }

            try
            {
                Received.Invoke(
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
            return new ReceivedMessage(metadata, () => new MemoryStream(body), _decrypter, _fileWriter);
        }
    }
}