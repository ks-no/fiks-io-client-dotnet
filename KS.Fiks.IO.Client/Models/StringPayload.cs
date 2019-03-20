using System.IO;
using System.Text;

namespace KS.Fiks.IO.Client.Models
{
    public class StringPayload : IPayload
    {
        public string Filename { get; set; }

        public Stream Payload => new MemoryStream(Encoding.UTF8.GetBytes(PayloadString));

        public string PayloadString { get; set; }
    }
}