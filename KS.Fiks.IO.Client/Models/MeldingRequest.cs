using System;
using System.Collections.Generic;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class MeldingRequest : MeldingBase
    {
        private const int DefaultTtlInDays = 2;

        public MeldingRequest(
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            string meldingType,
            TimeSpan? ttl = null,
            Dictionary<string, string> headere = null,
            Guid? svarPaMelding = null,
            Guid? klientMeldingId = null)
        : base(
            meldingId: Guid.Empty,
            meldingType: meldingType,
            klientMeldingId: klientMeldingId,
            avsenderKontoId: avsenderKontoId,
            mottakerKontoId: mottakerKontoId,
            ttl: ttl ?? TimeSpan.FromDays(DefaultTtlInDays),
            headere: headere ?? new Dictionary<string, string>(),
            svarPaMelding: svarPaMelding)
        {
        }

        public MeldingSpesifikasjonApiModel ToApiModel()
        {
            if (KlientMeldingId != null && KlientMeldingId != Guid.Empty)
            {
                Headere.Add(headerKlientMeldingId, KlientMeldingId.ToString());
            }

            return new MeldingSpesifikasjonApiModel(
                avsenderKontoId: AvsenderKontoId,
                mottakerKontoId: MottakerKontoId,
                meldingType: MeldingType,
                ttl: (long)Ttl.TotalMilliseconds,
                headere: Headere,
                svarPaMelding: SvarPaMelding);
        }
    }
}