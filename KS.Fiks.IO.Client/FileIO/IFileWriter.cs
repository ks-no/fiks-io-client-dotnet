using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    internal interface IFileWriter
    {
        void Write(string path, Stream data);

        void Write(string path, byte[] data);
    }
}