using System.IO;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsicDecrypter
    {
        Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath);

        Task<Stream> Decrypt(Task<Stream> encryptedZipStream);
    }
}