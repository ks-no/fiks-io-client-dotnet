using System;
using System.Collections.Generic;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class SendtMelding : MeldingBase
    {
        public static SendtMelding FromSentMessageApiModel(SendtMeldingApiModel sendtMeldingApiModel)
        {
            Guid? klientMeldingId = null;
            if (sendtMeldingApiModel.Headere != null && sendtMeldingApiModel.Headere.ContainsKey(headerKlientMeldingId))
            {
                try
                {
                    klientMeldingId = Guid.Parse(sendtMeldingApiModel.Headere[headerKlientMeldingId]);
                }
                catch (Exception e)
                {
                    klientMeldingId = Guid.Empty;
                }
            }

            return new SendtMelding(
                sendtMeldingApiModel.MeldingId,
                klientMeldingId,
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