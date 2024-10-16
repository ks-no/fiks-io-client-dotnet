using System;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Crypto.Asic;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private readonly IFileWriter _fileWriter;

        private readonly IAsicDecrypter _decrypter;

        private readonly ISendHandler _sendHandler;

        private readonly IDokumentlagerHandler _dokumentlagerHandler;

        private readonly Guid _accountId;

        public AmqpConsumerFactory(
            ISendHandler sendHandler,
            IDokumentlagerHandler dokumentlagerHandler,
            KontoConfiguration kontoConfiguration)
        {
            _dokumentlagerHandler = dokumentlagerHandler;
            _fileWriter = new FileWriter();
            _decrypter = new AsicDecrypter(DecryptionService.Create(kontoConfiguration.PrivatNokler));

            _sendHandler = sendHandler;
            _accountId = kontoConfiguration.KontoId;
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, _dokumentlagerHandler, _fileWriter, _decrypter, _sendHandler, _accountId);
        }
    }
}