using System;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessageMetadata : MessageBase
    {
        public ReceivedMessageMetadata(Guid messageId, string messageType, Guid receiverAccountId, Guid senderAccountId, Guid? relatedMessageId, TimeSpan ttl)
        {
            MessageId = messageId;
            MessageType = messageType;
            ReceiverAccountId = receiverAccountId;
            SenderAccountId = senderAccountId;
            RelatedMessageId = relatedMessageId;
            Ttl = ttl;
        }

        public ReceivedMessageMetadata(ReceivedMessageMetadata metadata)
            : base(metadata)
        {
            RelatedMessageId = metadata.RelatedMessageId;
        }
    }
}