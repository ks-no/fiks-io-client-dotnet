using System;
using System.Collections.Generic;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class SendtMelding : MeldingBase
    {
        public static SendtMelding FromSentMessageApiModel(SendtMeldingApiModel sendtMeldingApiModel)
        {
            return new SendtMelding(
                sendtMeldingApiModel.MeldingId,
                sendtMeldingApiModel.KlientMeldingID,
                sendtMeldingApiModel.MeldingType,
                sendtMeldingApiModel.AvsenderKontoId,
                sendtMeldingApiModel.MottakerKontoId,
                TimeSpan.FromMilliseconds(sendtMeldingApiModel.Ttl),
                sendtMeldingApiModel.Headere,
                sendtMeldingApiModel.SvarPaMelding);
        }

        internal SendtMelding(
            Guid meldingId,
            Guid? klientMeldingId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl,
            Dictionary<string, string> headere,
            Guid? svarPaMelding=null)
            : base(meldingId, klientMeldingId, meldingType, avsenderKontoId, mottakerKontoId, ttl, svarPaMelding: svarPaMelding)
        {
        }
    }
}