using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface ISvarSender
    {
        Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads, Guid? klientMeldingId = default);

        Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn, Guid? klientMeldingId = default);

        Task<SendtMelding> Svar(string meldingType, string melding, string filnavn, Guid? klientMeldingId = default);

        Task<SendtMelding> Svar(string meldingType, string filLokasjon, Guid? klientMeldingId = default);

        Task<SendtMelding> Svar(string meldingType, Guid? klientMeldingId = default);

        /**
         * Acknowledges that the message has been consumed
         */
        void Ack();

        /**
         * Acknowledges that the message could not be consumed
         */
        void Nack();

        /**
         *  Acknowledges that the message could not be consumed right and puts it back in the queue to be consumed again
         */
        void NackWithRequeue();
    }
}