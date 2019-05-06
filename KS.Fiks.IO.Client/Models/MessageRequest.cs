using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class MessageRequest
    {
        private const int DefaultTtlInDays = 2;

        public MessageRequest()
        {
            Ttl = TimeSpan.FromDays(DefaultTtlInDays);
        }

        public Guid SenderAccountId { get; set; }

        public Guid ReceiverAccountId { get; set; }

        public string MessageType { get; set; }

        public TimeSpan Ttl { get; set; }

        public Guid RelatedMessageId { get; set; }

        public MessageSpecificationApiModel ToApiModel()
        {
            return new MessageSpecificationApiModel
            {
                SenderAccountId = SenderAccountId,
                ReceiverAccountId = ReceiverAccountId,
                RelatedMessageId = RelatedMessageId,
                Ttl = (long)Ttl.TotalMilliseconds
            };
        }
    }
}