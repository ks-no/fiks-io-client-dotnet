using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    internal interface IFileWriter
    {
        void Write(Stream data, string path);
    }
}