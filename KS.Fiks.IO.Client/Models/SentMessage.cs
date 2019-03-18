using System;
using KS.Fiks.Io.Send.Client;

namespace KS.Fiks.IO.Client.Models
{
    public class SentMessage : IMessage
    {
        public Guid? MessageId { get; set; }

        public string MessageType { get; set; }

        public Guid? SenderAccountId { get; set; }

        public Guid? ReceiverAccountId { get; set; }

        public TimeSpan Ttl { get; set; }

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