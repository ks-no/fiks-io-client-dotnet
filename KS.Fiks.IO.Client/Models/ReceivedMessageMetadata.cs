using System;

namespace KS.Fiks.IO.Client.Models
{
    public class ReceivedMessageMetadata : MessageBase
    {
        public ReceivedMessageMetadata()
        {
        }

        public ReceivedMessageMetadata(ReceivedMessageMetadata metadata)
            : base(metadata)
        {
            RelatedMessageId = metadata.RelatedMessageId;
        }

        public Guid? RelatedMessageId { get; set; }
    }
}