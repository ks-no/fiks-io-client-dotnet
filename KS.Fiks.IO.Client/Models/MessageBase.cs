using System;

namespace KS.Fiks.IO.Client.Models
{
    public abstract class MessageBase : IMessage
    {
        protected MessageBase()
        {
        }

        protected MessageBase(IMessage message)
        {
            MessageId = message.MessageId;
            MessageType = message.MessageType;
            SenderAccountId = message.SenderAccountId;
            ReceiverAccountId = message.ReceiverAccountId;
            Ttl = message.Ttl;
        }

        public Guid? MessageId { get; set; }

        public string MessageType { get; set; }

        public Guid? SenderAccountId { get; set; }

        public Guid? ReceiverAccountId { get; set; }

        public TimeSpan? Ttl { get; set; }
    }
}