using System.Collections.Generic;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface ISendHandler
    {
        Task<SentMessage> Send(MessageRequest request, IEnumerable<IPayload> payload);
    }
}