using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client
{
    public interface IFiksIOClient : IDisposable
    {
        Guid KontoId { get; }

        Task<Konto> Lookup(LookupRequest request);

        Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload);

        Task<SendtMelding> Send(MeldingRequest request, string pathToPayload);

        Task<SendtMelding> Send(MeldingRequest request, string payload, string filename);

        Task<SendtMelding> Send(MeldingRequest request, Stream payload, string filename);

        void NewSubscription(EventHandler<MottattMeldingArgs> onMotattMelding);

        void NewSubscription(EventHandler<MottattMeldingArgs> onMotattMelding, EventHandler<ConsumerEventArgs> onCanceled);
    }
}
