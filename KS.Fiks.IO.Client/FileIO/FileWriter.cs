using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.FileIO
{
    internal class FileWriter : IFileWriter
    {
        private const int DefaultBufferSize = 81920;

        public async Task WriteAsync(Stream data, string path, CancellationToken cancellationToken = default)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await data.CopyToAsync(file, DefaultBufferSize, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}