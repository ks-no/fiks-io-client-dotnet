using System;
using System.Collections.Generic;

namespace KS.Fiks.IO.Client.Models
{
    public abstract class MeldingBase : IMelding
    {
        public const string headerKlientMeldingId = "klientMeldingId";

        protected MeldingBase()
        {
        }

        protected MeldingBase(
            Guid meldingId,
            Guid? klientMeldingId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl,
            bool resendt = false,
            Dictionary<string, string> headere = null,
            Guid? svarPaMelding = null)
        {
            MeldingId = meldingId;
            KlientMeldingId = klientMeldingId;
            MeldingType = meldingType;
            AvsenderKontoId = avsenderKontoId;
            MottakerKontoId = mottakerKontoId;
            Ttl = ttl;
            Headere = headere ?? new Dictionary<string, string>();
            SvarPaMelding = svarPaMelding;
            Resendt = resendt;
        }

        protected MeldingBase(IMelding melding)
        {
            MeldingId = melding.MeldingId;
            KlientMeldingId = melding.KlientMeldingId;
            MeldingType = melding.MeldingType;
            AvsenderKontoId = melding.AvsenderKontoId;
            MottakerKontoId = melding.MottakerKontoId;
            Ttl = melding.Ttl;
            Headere = melding.Headere;
            Resendt = melding.Resendt;
        }

        public Guid MeldingId { get; protected set; }

        public Guid? KlientMeldingId { get; protected set; }

        public string MeldingType { get; protected set; }

        public Guid AvsenderKontoId { get; protected set; }

        public Guid MottakerKontoId { get; protected set; }

        public TimeSpan Ttl { get; protected set; }

        public Dictionary<string, string> Headere { get; protected set; }

        public Guid? SvarPaMelding { get; protected set; }

        public bool Resendt { get; protected set; }
    }
}