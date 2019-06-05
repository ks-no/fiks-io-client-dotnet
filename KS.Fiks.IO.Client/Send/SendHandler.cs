using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Send.Client;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Send
{
    internal class SendHandler : ISendHandler
    {
        private readonly IFiksIOSender _sender;

        private readonly IAsicEncrypter _asicEncrypter;

        private readonly ICatalogHandler _catalogHandler;

        public SendHandler(ICatalogHandler catalogHandler, IFiksIOSender sender, IAsicEncrypter asicEncrypter)
        {
            _sender = sender;
            _asicEncrypter = asicEncrypter;
            _catalogHandler = catalogHandler;
        }

        public SendHandler(
            ICatalogHandler catalogHandler,
            IMaskinportenClient maskinportenClient,
            FiksIOSenderConfiguration senderConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            IAsicEncrypter asicEncrypter)
            : this(
                catalogHandler,
                new FiksIOSender(senderConfiguration, maskinportenClient, integrasjonConfiguration.IntegrasjonId, integrasjonConfiguration.IntegrasjonPassord),
                asicEncrypter)
        {
        }

        public async Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload)
        {
            var encryptedPayload = await GetEncryptedPayload(request, payload).ConfigureAwait(false);
            encryptedPayload.Seek(0, SeekOrigin.Begin);
            var sentMessageApiModel = await _sender.Send(request.ToApiModel(), encryptedPayload)
                                                   .ConfigureAwait(false);

            return SendtMelding.FromSentMessageApiModel(sentMessageApiModel);
        }

        private async Task<Stream> GetEncryptedPayload(MeldingRequest request, IList<IPayload> payload)
        {
            var publicKey = await _catalogHandler.GetPublicKey(request.MottakerKontoId).ConfigureAwait(false);
            return _asicEncrypter.Encrypt(publicKey, payload);
        }
    }
}