using System;

namespace KS.Fiks.IO.Client.Models
{
    public abstract class MessageBase : IMessage
    {
        protected MessageBase()
        {
        }

        protected MessageBase(
            Guid messageId,
            string messageType,
            Guid senderAccountId,
            Guid receiverAccountId,
            TimeSpan ttl,
            Guid? relatedMessageId = null)
        {
            MessageId = messageId;
            MessageType = messageType;
            SenderAccountId = senderAccountId;
            ReceiverAccountId = receiverAccountId;
            Ttl = ttl;
            RelatedMessageId = relatedMessageId;
        }

        protected MessageBase(IMessage message)
        {
            MessageId = message.MessageId;
            MessageType = message.MessageType;
            SenderAccountId = message.SenderAccountId;
            ReceiverAccountId = message.ReceiverAccountId;
            Ttl = message.Ttl;
        }

        public Guid MessageId { get; protected set; }

        public string MessageType { get; protected set; }

        public Guid SenderAccountId { get; protected set; }

        public Guid ReceiverAccountId { get; protected set; }

        public TimeSpan Ttl { get; protected set; }

        public Guid? RelatedMessageId { get; protected set; }
    }
}