using System;
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

        private readonly string _accountId;

        public AmqpReceiveConsumer(
            IModel model,
            IFileWriter fileWriter,
            IAsicDecrypter decrypter,
            ISendHandler sendHandler,
            string accountId)
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
            Console.WriteLine($"Data received with length: ${body.Length}");
            
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var receivedMessage = ParseMessage(properties, body);

            if (Received != null)
            {
                try
                {
                    Received.Invoke(
                        this,
                        new MessageReceivedArgs(receivedMessage, new ReplySender(_sendHandler, receivedMessage)));
                    Model.BasicAck(deliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        private ReceivedMessage ParseMessage(IBasicProperties properties, byte[] body)
        {
            var metadata = ReceivedMessageParser.Parse(_accountId, properties);
            return new ReceivedMessage(metadata, body, _decrypter, _fileWriter);
        }
    }
}