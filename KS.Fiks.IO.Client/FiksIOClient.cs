using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using Ks.Fiks.Maskinporten.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client
{
    public class FiksIOClient : IFiksIOClient
    {
        private readonly ICatalogHandler _catalogHandler;

        private readonly IMaskinportenClient _maskinportenClient;

        private readonly ISendHandler _sendHandler;

        private readonly IAmqpHandler _amqpHandler;

        public FiksIOClient(
            FiksIOConfiguration configuration,
            ICatalogHandler catalogHandler = null,
            IMaskinportenClient maskinportenClient = null,
            ISendHandler sendHandler = null,
            IAmqpHandler amqpHandler = null)
        {
            configuration = ConfigurationNormalizer.SetDefaultValues(configuration);

            AccountId = configuration.AccountConfiguration.AccountId;
            _maskinportenClient = maskinportenClient ?? new MaskinportenClient(configuration.MaskinportenConfiguration);
            _catalogHandler = catalogHandler ?? new CatalogHandler(configuration, _maskinportenClient);
            _sendHandler = sendHandler;
            _amqpHandler = amqpHandler;
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