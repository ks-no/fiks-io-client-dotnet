using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Send;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private readonly IFileWriter _fileWriter;

        private readonly IAsicDecrypter _decrypter;

        private readonly ISendHandler _sendHandler;

        private readonly string _accountId;

        public AmqpConsumerFactory(ISendHandler sendHandler, string accountId)
        {
            _fileWriter = new FileWriter();
            _decrypter = new AsicDecrypter();
            _sendHandler = sendHandler;
            _accountId = accountId;
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, _fileWriter, _decrypter, _sendHandler, _accountId);
        }
    }
}