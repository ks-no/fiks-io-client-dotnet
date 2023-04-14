using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Dokumentlager;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

[assembly: InternalsVisibleTo("KS.Fiks.IO.Client.Tests, PublicKey=002400000480000014010000060200000024000052534131000800000100010089a68f3fecb97831d694aff0c0108cfe2e11a96516448ed41db22281454d59eb3b18ca24f54dafc23e021e172399ec611b5c5195e481529ff2c17f5c72b0f9a438a7b386963bb70b560eb33fa08cb6b01604d76658ae3c151109b493a0dd4dc63789a84ac13e74bf7734843ce6065a36c2a3a27dc8395f96cd3c261c8db8275f9d270a3160ed0e36178908ab11b50663ed874dbe570303e44199b32ad1c0eee81286498d6fc4b24df661e1b359d9254d9118dda111d5f8bb0327e1584e1ad3260cad4e3a59b3898db7a6d129fa99156da7e2cad4282ad921cf26cb27d5951157ea6ccc572f198d9f7fb837c546dfa73f4a285423826de10eb8684cbbf26c3c93")]

namespace KS.Fiks.IO.Client
{
    public class FiksIOClient : IFiksIOClient
    {
        private readonly ICatalogHandler _catalogHandler;

        private ISendHandler _sendHandler;

        private IAmqpHandler _amqpHandler;

        private IDokumentlagerHandler _dokumentlagerHandler;

        private readonly IPublicKeyProvider _publicKeyProvider;

        private IMaskinportenClient _maskinportenClient;

        private FiksIOClient(
            FiksIOConfiguration configuration,
            HttpClient httpClient = null,
            IPublicKeyProvider publicKeyProvider = null)
            : this(configuration, null, null, null, null, null, httpClient, publicKeyProvider)
        {
        }

        private FiksIOClient(
            FiksIOConfiguration configuration,
            ICatalogHandler catalogHandler = null,
            IMaskinportenClient maskinportenClient = null,
            ISendHandler sendHandler = null,
            IDokumentlagerHandler dokumentlagerHandler = null,
            IAmqpHandler amqpHandler = null,
            HttpClient httpClient = null,
            IPublicKeyProvider publicKeyProvider = null,
            IAsicEncrypter asicEncrypter = null)
        {
            KontoId = configuration.KontoConfiguration.KontoId;

            _maskinportenClient = maskinportenClient ?? new MaskinportenClient(configuration.MaskinportenConfiguration, httpClient);

            _catalogHandler = catalogHandler ?? new CatalogHandler(
                                  configuration.KatalogConfiguration,
                                  configuration.IntegrasjonConfiguration,
                                  _maskinportenClient,
                                  httpClient);

            _publicKeyProvider = publicKeyProvider ?? new CatalogPublicKeyProvider(_catalogHandler);

            var _asicEncrypter = asicEncrypter ?? new AsicEncrypter(
                     new AsiceBuilderFactory(), 
                     new EncryptionServiceFactory(), 
                     AsicSigningCertificateHolderFactory.Create(configuration.AsiceSigningConfiguration));

            _sendHandler = sendHandler ??
                           new SendHandler(
                               _catalogHandler,
                               _maskinportenClient,
                               configuration.FiksIOSenderConfiguration,
                               configuration.IntegrasjonConfiguration,
                               httpClient,
                               _asicEncrypter,
                               _publicKeyProvider);

            _dokumentlagerHandler = dokumentlagerHandler ?? new DokumentlagerHandler(
                configuration.DokumentlagerConfiguration,
                configuration.IntegrasjonConfiguration,
                _maskinportenClient,
                httpClient: httpClient);

            _amqpHandler = amqpHandler;
        }

        public static async Task<FiksIOClient> CreateAsync(FiksIOConfiguration configuration, HttpClient httpClient = null, IPublicKeyProvider publicKeyProvider = null, ILogger logger = null)
        {
            var client = new FiksIOClient(configuration, httpClient, publicKeyProvider);
            await client.InitializeAsync(configuration, logger).ConfigureAwait(false);

            return client;
        }

        internal static async Task<FiksIOClient> CreateAsync(FiksIOConfiguration configuration,
            ICatalogHandler catalogHandler = null,
            IMaskinportenClient maskinportenClient = null,
            ISendHandler sendHandler = null,
            IDokumentlagerHandler dokumentlagerHandler = null,
            IAmqpHandler amqpHandler = null,
            HttpClient httpClient = null,
            IPublicKeyProvider publicKeyProvider = null,
            IAsicEncrypter asicEncrypter = null,
            ILogger logger = null)
        {
            var client = new FiksIOClient(
                configuration,
                catalogHandler, 
                maskinportenClient, 
                sendHandler, 
                dokumentlagerHandler, 
                amqpHandler,
                httpClient, 
                publicKeyProvider,
                asicEncrypter);

            await client.InitializeAsync(configuration, logger).ConfigureAwait(false);

            return client;
        }

        private async Task InitializeAsync(FiksIOConfiguration configuration, ILogger logger= null)
        {
            _amqpHandler = _amqpHandler ?? await AmqpHandler.CreateAsync(_maskinportenClient,
                _sendHandler,
                _dokumentlagerHandler,
                configuration.AmqpConfiguration,
                configuration.IntegrasjonConfiguration,
                configuration.KontoConfiguration,
                null,
                null,
                logger);
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

        public bool IsOpen()
        {
            return _amqpHandler.IsOpen();
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
            return await Send(request, new List<IPayload> { payload }).ConfigureAwait(false);
        }
    }
}