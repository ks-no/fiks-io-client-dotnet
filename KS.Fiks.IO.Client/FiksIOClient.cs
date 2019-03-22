using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Encryption;
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
                                  configuration.FiksIntegrationConfiguration,
                                  maskinportenClient);

            _sendHandler = sendHandler ??
                           new SendHandler(
                               _catalogHandler,
                               maskinportenClient,
                               configuration.FiksIOSenderConfiguration,
                               configuration.FiksIntegrationConfiguration,
                               new DummyCrypt());

            _amqpHandler = amqpHandler ?? new AmqpHandler(configuration.AmqpConfiguration, configuration.FiksIntegrationConfiguration, AccountId);
        }

        public string AccountId { get; }

        public async Task<Account> Lookup(LookupRequest request)
        {
            return await _catalogHandler.Lookup(request).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, IEnumerable<IPayload> payload)
        {
            return await _sendHandler.Send(request, payload).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, string pathToPayload)
        {
            var payloadList = new List<IPayload>
            {
                new FilePayload(pathToPayload)
            };

            return await Send(request, payloadList).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, string payload, string filename)
        {
            var payloadList = new List<IPayload>
            {
                new StringPayload
                {
                    PayloadString = payload,
                    Filename = filename
                }
            };

            return await Send(request, payloadList).ConfigureAwait(false);
        }

        public async Task<SentMessage> Send(MessageRequest request, Stream payload, string filename)
        {
            var payloadList = new List<IPayload>
            {
                new StreamPayload
                {
                    Payload = payload,
                    Filename = filename
                }
            };

            return await Send(request, payloadList).ConfigureAwait(false);
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
    }
}