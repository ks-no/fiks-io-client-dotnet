using System.IO;

namespace KS.Fiks.IO.Client.Models
{
    public interface IPayload
    {
        string Filename { get; }

        Stream Payload { get; }
    }
}