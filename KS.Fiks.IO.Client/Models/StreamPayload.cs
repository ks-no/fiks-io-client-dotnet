using System.IO;

namespace KS.Fiks.IO.Client.Models
{
    public class StreamPayload : IPayload
    {
        public string Filename { get; set; }

        public Stream Payload { get; set; }
    }
}