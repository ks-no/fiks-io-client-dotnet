using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageRequest : MessageBase
    {
        private const int DefaultTtlInDays = 2;

        public MessageRequest(
            Guid senderAccountId,
            Guid receiverAccountId,
            string messageType,
            TimeSpan? ttl = null,
            Guid? relatedMessageId = null)
        : base(Guid.Empty, messageType, senderAccountId, receiverAccountId, ttl ?? TimeSpan.FromDays(DefaultTtlInDays), relatedMessageId)
        {
        }

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