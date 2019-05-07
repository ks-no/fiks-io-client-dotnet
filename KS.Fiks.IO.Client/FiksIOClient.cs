using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client.Events;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("KS.Fiks.IO.Client.Tests")]

namespace KS.Fiks.IO.Client
{
    public class FiksIOClient : IFiksIOClient
    {
        private readonly ICatalogHandler _catalogHandler;

        private readonly ISendHandler _sendHandler;

        private readonly IAmqpHandler _amqpHandler;

        public FiksIOClient(FiksIOConfiguration configuration)
            : this(configuration, null, null, null, null)
        {
        }

        internal FiksIOClient(
            FiksIOConfiguration configuration,
            ICatalogHandler catalogHandler,
            IMaskinportenClient maskinportenClient,
            ISendHandler sendHandler,
            IAmqpHandler amqpHandler)
        {
            AccountId = configuration.AccountConfiguration.AccountId;

            maskinportenClient = maskinportenClient ?? new MaskinportenClient(configuration.MaskinportenConfiguration);

            _catalogHandler = catalogHandler ?? new CatalogHandler(
                                  configuration.CatalogConfiguration,
                                  configuration.IntegrationConfiguration,
                                  maskinportenClient);

            var asicEncrypter = new AsicEncrypter(new AsiceBuilderFactory(), new EncryptionServiceFactory());

            _sendHandler = sendHandler ??
                           new SendHandler(
                               _catalogHandler,
                               maskinportenClient,
                               configuration.FiksIOSenderConfiguration,
                               configuration.IntegrationConfiguration,
                               asicEncrypter);

            _amqpHandler = amqpHandler ?? new AmqpHandler(
                               maskinportenClient,
                               _sendHandler,
                               configuration.AmqpConfiguration,
                               configuration.IntegrationConfiguration,
                               configuration.AccountConfiguration);
        }

        public Guid AccountId { get; }

        public async Task<Account> Lookup(LookupRequest request)
        {
            return await _catalogHandler.Lookup(request).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, IList<IPayload> payload)
        {
            return await _sendHandler.Send(request, payload).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, string pathToPayload)
        {
            return await Send(request, new FilePayload(pathToPayload)).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, string payload, string filename)
        {
            return await Send(request, new StringPayload(payload, filename)).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, Stream payload, string filename)
        {
            return await Send(request, new StreamPayload(payload, filename)).ConfigureAwait(false);
        }

        public void NewSubscription(EventHandler<MessageReceivedArgs> onReceived)
        {
            NewSubscription(onReceived, null);
        }

        public void NewSubscription(
            EventHandler<MessageReceivedArgs> onReceived,
            EventHandler<ConsumerEventArgs> onCanceled)
        {
            _amqpHandler.AddMessageReceivedHandler(onReceived, onCanceled);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _amqpHandler?.Dispose();
            }
        }

        private async Task<SentMessage> Send(MessageRequest request, IPayload payload)
        {
            return await Send(request, new List<IPayload> {payload}).ConfigureAwait(false);
        }
    }
}