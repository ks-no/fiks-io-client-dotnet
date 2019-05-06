using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class SentMessage : MessageBase
    {
        public static SentMessage FromSentMessageApiModel(SentMessageApiModel sentMessageApiModel)
        {
            return new SentMessage
            {
                MessageType = sentMessageApiModel.MessageType,
                MessageId = sentMessageApiModel.MessageId,
                SenderAccountId = sentMessageApiModel.SenderAccountId,
                ReceiverAccountId = sentMessageApiModel.ReceiverAccountId,
                Ttl = TimeSpan.FromMilliseconds(sentMessageApiModel.Ttl)
            };
        }
    }
}