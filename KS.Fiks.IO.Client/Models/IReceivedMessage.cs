using System;
using System.IO;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Models
{
    public interface IReceivedMessage : IMessage
    {
        Guid? RelatedMessageId { get; }

        Task<Stream> EncryptedStream { get;  }

        Task<Stream> DecryptedStream { get;  }

        Task WriteEncryptedZip(string outPath);

        Task WriteDecryptedZip(string outPath);
    }
}