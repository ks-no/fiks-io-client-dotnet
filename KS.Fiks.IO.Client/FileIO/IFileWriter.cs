using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.FileIO
{
    internal interface IFileWriter
    {
        Task WriteAsync(Stream data, string path, CancellationToken cancellationToken = default);
    }
}