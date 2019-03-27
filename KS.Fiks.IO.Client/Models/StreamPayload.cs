using System.IO;
using KS.Fiks.IO.Client.Configuration;

namespace KS.Fiks.IO.Client.Models
{
    public class StreamPayload : IPayload
    {
        public StreamPayload(Stream payload, string filename)
        {
            Payload = payload;
            Filename = filename;
        }

        public string Filename { get; }

        public Stream Payload { get; }
    }
}