using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.Io.Send.Client;

namespace KS.Fiks.IO.Client.Send
{
    public class SendHandler : ISendHandler
    {
        private readonly IFiksIOSender _sender;

        private readonly IPayloadEncrypter _payloadEncrypter;

        private readonly ICatalogHandler _catalogHandler;

        public SendHandler(IFiksIOSender sender, IPayloadEncrypter payloadEncrypter, ICatalogHandler catalogHandler)
        {
            _sender = sender;
            _payloadEncrypter = payloadEncrypter;
            _catalogHandler = catalogHandler;
        }

        public async Task<SentMessage> Send(MessageRequest request, IEnumerable<IPayload> payload)
        {
            var encryptedPayload = await GetEncryptedPayload(request, payload).ConfigureAwait(false);

            var sentMessageApiModel = await _sender.Send(request.ToApiModel(), encryptedPayload)
                                                   .ConfigureAwait(false);

            return SentMessage.FromSentMessageApiModel(sentMessageApiModel);
        }

        private async Task<Stream> GetEncryptedPayload(MessageRequest request, IEnumerable<IPayload> payload)
        {
            var publicKey = await _catalogHandler.GetPublicKey(request.ReceiverAccountId).ConfigureAwait(false);
            return _payloadEncrypter.Encrypt(publicKey, payload);
        }
    }
}