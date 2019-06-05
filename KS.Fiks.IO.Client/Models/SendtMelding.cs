using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class SendtMelding : MeldingBase
    {
        public static SendtMelding FromSentMessageApiModel(SendtMeldingApiModel sendtMeldingApiModel)
        {
            return new SendtMelding(
                sendtMeldingApiModel.MeldingId,
                sendtMeldingApiModel.MeldingType,
                sendtMeldingApiModel.AvsenderKontoId,
                sendtMeldingApiModel.MottakerKontoId,
                TimeSpan.FromMilliseconds(sendtMeldingApiModel.Ttl));
        }

        internal SendtMelding(
            Guid meldingId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl)
            : base(meldingId, meldingType, avsenderKontoId, mottakerKontoId, ttl)
        {
        }
    }
}