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
            Guid? klientMeldingId = null,
            string klientKorrelasjonsId = null)
            : base(
                meldingId: Guid.Empty,
                meldingType: meldingType,
                klientMeldingId: klientMeldingId,
                klientKorrelasjonsId: klientKorrelasjonsId,
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
                Headere.Add(HeaderKlientMeldingId, KlientMeldingId.ToString());
            }

            if (!string.IsNullOrEmpty(KlientKorrelasjonsId))
            {
                Headere.Add(HeaderKlientKorrelasjonsId, KlientKorrelasjonsId);
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