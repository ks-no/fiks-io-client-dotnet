using System;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    public class AmqpReceiveConsumer : DefaultBasicConsumer, IAmqpReceiveConsumer
    {
        private readonly IPayloadDecrypter _decrypter;

        private readonly IFileWriter _fileWriter;

        public AmqpReceiveConsumer(IModel model, IFileWriter fileWriter, IPayloadDecrypter decrypter)
            : base(model)
        {
            _fileWriter = fileWriter;
            _decrypter = decrypter;
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

            var receivedMessage = ParseMessage(routingKey, properties, body);

            Received?.Invoke(
                this,
                new MessageReceivedArgs(receivedMessage, new ResponseSender()));
        }

        private ReceivedMessage ParseMessage(string routingKey, IBasicProperties properties, byte[] body)
        {
            var metadata = ReceivedMessageParser.Parse(routingKey, properties);
            return new ReceivedMessage(metadata, body, _decrypter, _fileWriter);
        }
    }
}