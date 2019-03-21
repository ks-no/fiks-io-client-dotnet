using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private readonly IFileWriter _fileWriter;

        private readonly IPayloadDecrypter _decrypter;

        public AmqpConsumerFactory()
        {
            _fileWriter = new FileWriter();
            _decrypter = new DummyCrypt();
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, _fileWriter, _decrypter);
        }
    }
}