using System;

namespace KS.Fiks.IO.Client.Models
{
    public abstract class MeldingBase : IMelding
    {
        protected MeldingBase()
        {
        }

        protected MeldingBase(
            Guid meldingId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl,
            Guid? svarPaMelding = null)
        {
            MeldingId = meldingId;
            MeldingType = meldingType;
            AvsenderKontoId = avsenderKontoId;
            MottakerKontoId = mottakerKontoId;
            Ttl = ttl;
            SvarPaMelding = svarPaMelding;
        }

        protected MeldingBase(IMelding melding)
        {
            MeldingId = melding.MeldingId;
            MeldingType = melding.MeldingType;
            AvsenderKontoId = melding.AvsenderKontoId;
            MottakerKontoId = melding.MottakerKontoId;
            Ttl = melding.Ttl;
        }

        public Guid MeldingId { get; protected set; }

        public string MeldingType { get; protected set; }

        public Guid AvsenderKontoId { get; protected set; }

        public Guid MottakerKontoId { get; protected set; }

        public TimeSpan Ttl { get; protected set; }

        public Guid? SvarPaMelding { get; protected set; }
    }
}