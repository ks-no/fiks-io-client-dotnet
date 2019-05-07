using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class SentMessage : MessageBase
    {
        public static SentMessage FromSentMessageApiModel(SentMessageApiModel sentMessageApiModel)
        {
            return new SentMessage(
                sentMessageApiModel.MessageId,
                sentMessageApiModel.MessageType,
                sentMessageApiModel.SenderAccountId,
                sentMessageApiModel.ReceiverAccountId,
                TimeSpan.FromMilliseconds(sentMessageApiModel.Ttl));
        }

        internal SentMessage(
            Guid messageId,
            string messageType,
            Guid senderAccountId,
            Guid receiverAccountId,
            TimeSpan ttl)
            : base(messageId, messageType, senderAccountId, receiverAccountId, ttl)
        {
        }
    }
}