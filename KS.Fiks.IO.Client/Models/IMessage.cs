using System;

namespace KS.Fiks.IO.Client.Models
{
    public interface IMessage
    {
        Guid MessageId { get; }

        string MessageType { get; }

        Guid SenderAccountId { get; }

        Guid ReceiverAccountId { get; }

        TimeSpan Ttl { get; }
    }
}
