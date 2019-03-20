using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private IFileWriter _fileWriter;

        private IPayloadDecrypter _decrypter;

        public AmqpConsumerFactory()
        {
            _fileWriter = new FileWriter();
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, _fileWriter, _decrypter);
        }
    }
}