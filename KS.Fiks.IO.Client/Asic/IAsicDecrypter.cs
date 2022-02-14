using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsicDecrypter
    {
        Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath);

        Task<Stream> Decrypt(Task<Stream> encryptedZipStream);

        Task<IEnumerable<IPayload>> DecryptAndExtractPayloads(Task<Stream> encryptedZipStream);
    }
}