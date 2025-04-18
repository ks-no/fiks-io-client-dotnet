using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface ISendHandler
    {
        Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload, CancellationToken cancellationToken = default);
    }
}