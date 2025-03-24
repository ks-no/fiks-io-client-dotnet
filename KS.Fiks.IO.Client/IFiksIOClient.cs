using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client
{
    public interface IFiksIOClient : IAsyncDisposable
    {
        Guid KontoId { get; }

        Task<Konto> Lookup(LookupRequest request);

        Task<Konto> GetKonto(Guid kontoId);

        Task<Status> GetKontoStatus(Guid kontoId);

        Task<SendtMelding> Send(MeldingRequest request, CancellationToken cancellationToken = default);

        Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload, CancellationToken cancellationToken = default);

        Task<SendtMelding> Send(MeldingRequest request, string pathToPayload, CancellationToken cancellationToken = default);

        Task<SendtMelding> Send(MeldingRequest request, string payload, string filename, CancellationToken cancellationToken = default);

        Task<SendtMelding> Send(MeldingRequest request, Stream payload, string filename, CancellationToken cancellationToken = default);

        Task NewSubscriptionAsync(Func<MottattMeldingArgs, Task> onMottattMelding, Func<ConsumerEventArgs, Task> onCanceled = null);

        Task<bool> IsOpenAsync();
    }
}
