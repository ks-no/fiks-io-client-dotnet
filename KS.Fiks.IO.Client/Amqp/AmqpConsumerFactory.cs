using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Send;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private readonly IFileWriter _fileWriter;

        private readonly IPayloadDecrypter _decrypter;

        private readonly ISendHandler _sendHandler;

        public AmqpConsumerFactory(ISendHandler sendHandler)
        {
            _fileWriter = new FileWriter();
            _decrypter = new DummyCrypt();
            _sendHandler = sendHandler;
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, _fileWriter, _decrypter, _sendHandler);
        }
    }
}