using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    internal class FileWriter : IFileWriter
    {
        public void Write(string path, Stream data)
        {
            using (var fileStream = File.Create(path))
            {
                data.Seek(0, SeekOrigin.Begin);
                data.CopyTo(fileStream);
            }
        }

        public void Write(string path, byte[] data)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
        }
    }
}