using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageRequest
    {
        private const int DefaultTtlInDays = 2;

        public MessageRequest(
            Guid senderAccountId,
            Guid receiverAccountId,
            string messageType,
            TimeSpan? ttl = null,
            Guid? relatedMessageId = null)
        {
            SenderAccountId = senderAccountId;
            ReceiverAccountId = receiverAccountId;
            MessageType = messageType;
            Ttl = ttl ?? TimeSpan.FromDays(DefaultTtlInDays);
            RelatedMessageId = RelatedMessageId;
        }

        public Guid SenderAccountId { get; }

        public Guid ReceiverAccountId { get; }

        public string MessageType { get; }

        public TimeSpan Ttl { get; }

        public Guid RelatedMessageId { get; }

        public MessageSpecificationApiModel ToApiModel()
        {
            return new MessageSpecificationApiModel(
                SenderAccountId,
                ReceiverAccountId,
                MessageType,
                (long)Ttl.TotalMilliseconds,
                RelatedMessageId);
        }
    }
}