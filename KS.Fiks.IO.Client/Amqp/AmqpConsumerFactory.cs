using System;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Send;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpConsumerFactory : IAmqpConsumerFactory
    {
        private readonly IFileWriter fileWriter;

        private readonly IAsicDecrypter decrypter;

        private readonly ISendHandler sendHandler;

        private readonly IDokumentlagerHandler dokumentlagerHandler;

        private readonly Guid accountId;

        public AmqpConsumerFactory(ISendHandler sendHandler, IDokumentlagerHandler dokumentlagerHandler, KontoConfiguration kontoConfiguration)
        {
            this.dokumentlagerHandler = dokumentlagerHandler;
            this.fileWriter = new FileWriter();
            this.decrypter = new AsicDecrypter(DecryptionService.Create(kontoConfiguration.PrivatNokkel));
            this.sendHandler = sendHandler;
            this.accountId = kontoConfiguration.KontoId;
        }

        public IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel)
        {
            return new AmqpReceiveConsumer(channel, this.dokumentlagerHandler, this.fileWriter, this.decrypter, this.sendHandler, this.accountId);
        }
    }
}