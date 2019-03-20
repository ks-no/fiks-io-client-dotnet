using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    public class FileWriter : IFileWriter
    {
        public void Write(string path, Stream data)
        {
            using (var file = new StreamWriter(path))
            {
                file.Write(data);
            }
        }

        public void Write(string path, byte[] data)
        {
            using (var file = new StreamWriter(path))
            {
                file.Write(data);
            }
        }
    }
}