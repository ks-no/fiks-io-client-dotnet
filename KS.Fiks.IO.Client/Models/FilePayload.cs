using System.IO;

namespace KS.Fiks.IO.Client.Models
{
    public class FilePayload : IPayload
    {
        private string _path;

        public FilePayload(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file {path}");
            }

            // Make sure file can be read. Will throw if not.
            File.Open(path, FileMode.Open, FileAccess.Read).Dispose();

            _path = path;
        }

        public string Filename => Path.GetFileName(_path);

        public Stream Payload => new FileStream(_path, FileMode.Open, FileAccess.Read);
    }
}