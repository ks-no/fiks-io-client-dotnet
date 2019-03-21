using System;
using KS.Fiks.IO.Send.Client;

namespace KS.Fiks.IO.Client.Models
{
    public class SentMessage : MessageBase
    {
        public static SentMessage FromSentMessageApiModel(SentMessageApiModel sentMessageApiModel)
        {
            return new SentMessage
            {
                MessageType = sentMessageApiModel.MeldingType,
                MessageId = sentMessageApiModel.MeldingId,
                SenderAccountId = sentMessageApiModel.AvsenderKontoId,
                ReceiverAccountId = sentMessageApiModel.MottakerKontoId,
                Ttl = TimeSpan.FromMilliseconds(sentMessageApiModel.Ttl)
            };
        }
    }
}