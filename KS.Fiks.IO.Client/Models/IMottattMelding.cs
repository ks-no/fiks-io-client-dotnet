using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Models
{
    public interface IMottattMelding : IMelding
    {
        bool HasPayload { get; }

        Guid? SvarPaMelding { get; }

        Task<Stream> EncryptedStream { get; }

        Task<Stream> DecryptedStream { get; }

        Task WriteEncryptedZip(string outPath);

        Task WriteDecryptedZip(string outPath);

        Task<IEnumerable<IPayload>> Payloads { get; }
    }
}