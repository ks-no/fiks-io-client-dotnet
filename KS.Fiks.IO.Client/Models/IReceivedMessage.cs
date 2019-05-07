using System;
using System.IO;

namespace KS.Fiks.IO.Client.Models
{
    public interface IReceivedMessage : IMessage
    {
        Guid? RelatedMessageId { get; set; }

        Stream EncryptedStream { get;  }

        Stream DecryptedStream { get;  }

        void WriteEncryptedZip(string outPath);

        void WriteDecryptedZip(string outPath);
    }
}