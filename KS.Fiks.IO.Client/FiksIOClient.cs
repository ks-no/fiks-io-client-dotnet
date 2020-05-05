using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
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

        private readonly IDokumentlagerHandler _dokumentlagerHandler;

        private readonly IPublicKeyProvider _publicKeyProvider;

        public FiksIOClient(FiksIOConfiguration configuration, IPublicKeyProvider publicKeyProvider = null)
            : this(configuration, null, null, null, null, null, publicKeyProvider)
        {
        }

        internal FiksIOClient(
            FiksIOConfiguration configuration,
            ICatalogHandler catalogHandler = null,
            IMaskinportenClient maskinportenClient = null,
            ISendHandler sendHandler = null,
            IDokumentlagerHandler dokumentlagerHandler = null,
            IAmqpHandler amqpHandler = null,
            IPublicKeyProvider publicKeyProvider = null)
        {
            KontoId = configuration.KontoConfiguration.KontoId;

            maskinportenClient = maskinportenClient ?? new MaskinportenClient(configuration.MaskinportenConfiguration);

            _catalogHandler = catalogHandler ?? new CatalogHandler(
                                  configuration.KatalogConfiguration,
                                  configuration.IntegrasjonConfiguration,
                                  maskinportenClient);

            _publicKeyProvider = publicKeyProvider ?? new CatalogPublicKeyProvider(_catalogHandler);

            var asicEncrypter = new AsicEncrypter(new AsiceBuilderFactory(), new EncryptionServiceFactory());

            _sendHandler = sendHandler ??
                           new SendHandler(
                               _catalogHandler,
                               maskinportenClient,
                               configuration.FiksIOSenderConfiguration,
                               configuration.IntegrasjonConfiguration,
                               asicEncrypter,
                               _publicKeyProvider);

            _dokumentlagerHandler = dokumentlagerHandler ?? new DokumentlagerHandler(configuration.DokumentlagerConfiguration, configuration.IntegrasjonConfiguration, maskinportenClient);

            _amqpHandler = amqpHandler ?? new AmqpHandler(
                               maskinportenClient,
                               _sendHandler,
                               _dokumentlagerHandler,
                               configuration.AmqpConfiguration,
                               configuration.IntegrasjonConfiguration,
                               configuration.KontoConfiguration);
        }

        public Guid KontoId { get; }

        public async Task<Konto> Lookup(LookupRequest request)
        {
            return await _catalogHandler.Lookup(request).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Send(MeldingRequest request)
        {
            return await Send(request, new List<IPayload>()).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload)
        {
            return await _sendHandler.Send(request, payload).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Send(MeldingRequest request, string pathToPayload)
        {
            return await Send(request, new FilePayload(pathToPayload)).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Send(MeldingRequest request, string payload, string filename)
        {
            return await Send(request, new StringPayload(payload, filename)).ConfigureAwait(false);
        }

        public async Task<SendtMelding> Send(MeldingRequest request, Stream payload, string filename)
        {
            return await Send(request, new StreamPayload(payload, filename)).ConfigureAwait(false);
        }

        public void NewSubscription(EventHandler<MottattMeldingArgs> onMottattMelding)
        {
            NewSubscription(onMottattMelding, null);
        }

        public void NewSubscription(
            EventHandler<MottattMeldingArgs> onMottattMelding,
            EventHandler<ConsumerEventArgs> onCanceled)
        {
            _amqpHandler.AddMessageReceivedHandler(onMottattMelding, onCanceled);
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

        private async Task<SendtMelding> Send(MeldingRequest request, IPayload payload)
        {
            return await Send(request, new List<IPayload> {payload}).ConfigureAwait(false);
        }
    }
}