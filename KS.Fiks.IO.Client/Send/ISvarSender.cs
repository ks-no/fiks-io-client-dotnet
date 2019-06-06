using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Send
{
    public interface ISvarSender
    {
        Task<SendtMelding> Svar(string meldingType, IList<IPayload> payloads);

        Task<SendtMelding> Svar(string meldingType, Stream melding, string filnavn);

        Task<SendtMelding> Svar(string meldingType, string melding, string filnavn);

        Task<SendtMelding> Svar(string meldingType, string filLokasjon);

        Task<SendtMelding> Svar(string meldingType);

        void Ack();
    }
}