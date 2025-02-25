using System;
using System.IO;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Dokumentlager
{
    public interface IDokumentlagerHandler
    {
        Task<Stream> Download(Guid messageId);
    }
}