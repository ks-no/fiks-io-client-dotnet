using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    internal class FileWriter : IFileWriter
    {
        public void Write(Stream data, string path)
        {
            var memoryStream = new MemoryStream();
            data.CopyTo(memoryStream);
            Write(path, memoryStream.ToArray());
        }

        private void Write(string path, byte[] data)
        {
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
        }
    }
}