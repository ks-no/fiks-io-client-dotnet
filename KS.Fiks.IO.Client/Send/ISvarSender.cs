using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface ISvarSender
    {
        Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads, Guid? klientMeldingId = default, CancellationToken cancellationToken = default);

        Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn, Guid? klientMeldingId = default, CancellationToken cancellationToken = default);

        Task<SendtMelding> Svar(string meldingType, string melding, string filnavn, Guid? klientMeldingId = default, CancellationToken cancellationToken = default);

        Task<SendtMelding> Svar(string meldingType, string filLokasjon, Guid? klientMeldingId = default, CancellationToken cancellationToken = default);

        Task<SendtMelding> Svar(string meldingType, Guid? klientMeldingId = default, CancellationToken cancellationToken = default);

        /**
        * Acknowledges that the message has been consumed
        */
        Task AckAsync();

        /**
        * Acknowledges that the message could not be consumed. Message goes to dead letter queue and leads to no error message back to the sender.
        */
        Task NackAsync();

        /**
        * Acknowledges that the message could not be consumed right and puts it back in the queue to be consumed again, but with no added counter to deliverycount
         * This means normally that the message will be redelivered until TTL is reached, and will not be sent to the dead letter queue until TTL is reached.
         * We have deprecated this method, but also changed it's behavior so it works like RejectWithRequeueAsync. Read more on RejectWithRequeueAsync.
       */
        [Obsolete("NackWithRequeueAsync is deprecated. Use RejectWithRequeueAsync instead.")]
        Task NackWithRequeueAsync();

        /**
        * Acknowledges that the message could not be consumed right and puts it back in the queue to be consumed again, with added counter to deliverycount
         * This means that the message will be redelivered until max delivery count is reached, and will be sent to the dead letter queue when max delivery count is reached.
         * Max delivery count reached leads to error message back to the sender.
       */
        Task RejectWithRequeueAsync();
    }
}
