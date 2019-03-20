using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    public interface IFileWriter
    {
        void Write(string path, Stream data);

        void Write(string path, byte[] data);
    }
}