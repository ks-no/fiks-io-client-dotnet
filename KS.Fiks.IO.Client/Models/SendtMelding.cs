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
            if (sendtMeldingApiModel.Headere != null && sendtMeldingApiModel.Headere.TryGetValue(HeaderKlientMeldingId, out var klientMeldingIdValue))
            {
                try
                {
                    klientMeldingId = Guid.Parse(klientMeldingIdValue);
                }
                catch (Exception e)
                {
                    klientMeldingId = Guid.Empty;
                }
            }

            string klientKorrelasjonsId = null;
            if (sendtMeldingApiModel.Headere != null && sendtMeldingApiModel.Headere.TryGetValue(HeaderKlientKorrelasjonsId, out var klientKorrelasjonsIdValue))
            {
                klientKorrelasjonsId = klientKorrelasjonsIdValue;
            }

            return new SendtMelding(
                sendtMeldingApiModel.MeldingId,
                klientMeldingId,
                klientKorrelasjonsId,
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
            string klientKorrelasjonsId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl,
            Dictionary<string, string> headere,
            Guid? svarPaMelding=null)
            : base(meldingId, klientMeldingId, klientKorrelasjonsId, meldingType, avsenderKontoId, mottakerKontoId, ttl, svarPaMelding: svarPaMelding)
        {
        }
    }
}