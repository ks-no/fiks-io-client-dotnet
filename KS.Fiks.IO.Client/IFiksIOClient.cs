using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client
{
    public interface IFiksIOClient : IDisposable
    {
        Guid KontoId { get; }

        Task<Konto> Lookup(LookupRequest request);

        Task<Konto> GetKonto(Guid kontoId);

        Task<Status> GetKontoStatus(Guid kontoId);

        Task<SendtMelding> Send(MeldingRequest request);

        Task<SendtMelding> Send(MeldingRequest request, IList<IPayload> payload);

        Task<SendtMelding> Send(MeldingRequest request, string pathToPayload);

        Task<SendtMelding> Send(MeldingRequest request, string payload, string filename);

        Task<SendtMelding> Send(MeldingRequest request, Stream payload, string filename);

        void NewSubscription(EventHandler<MottattMeldingArgs> onMottattMelding);

        void NewSubscription(EventHandler<MottattMeldingArgs> onMottattMelding, EventHandler<ConsumerEventArgs> onCanceled);

        bool IsOpen();
    }
}
