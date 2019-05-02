using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface IReplySender
    {
        Task<SentMessage> Reply(string messageType, IList<IPayload> payloads);

        Task<SentMessage> Reply(string messageType, Stream message, string filename);

        Task<SentMessage> Reply(string messageType, string message, string filename);

        Task<SentMessage> Reply(string messageType, string filePath);

        Task<SentMessage> Reply(string messageType);

        void Ack();
    }
}