using System;

namespace KS.Fiks.IO.Client.Models
{
    public interface IMessage
    {
        Guid MessageId { get; set; }

        string MessageType { get; set; }

        Guid SenderAccountId { get; set; }

        Guid ReceiverAccountId { get; set; }

        TimeSpan Ttl { get; set; }
    }
}
