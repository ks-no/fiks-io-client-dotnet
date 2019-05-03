using System;
using System.IO;

namespace KS.Fiks.IO.Client.FileIO
{
    internal class FileWriter : IFileWriter
    {
        public void Write(string path, Stream data)
        {
            var streamData = new byte[data.Length];
            data.Write(streamData, 0, Convert.ToInt32(data.Length));
            Write(path, streamData);
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