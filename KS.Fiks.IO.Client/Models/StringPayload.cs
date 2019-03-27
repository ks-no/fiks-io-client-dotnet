using System.IO;
using System.Text;

namespace KS.Fiks.IO.Client.Models
{
    public class StringPayload : IPayload
    {
        private readonly string _payload;

        public StringPayload(string payload, string filename)
        {
            _payload = payload;
            Filename = filename;
        }

        public string Filename { get; }

        public Stream Payload => new MemoryStream(Encoding.UTF8.GetBytes(_payload));
    }
}