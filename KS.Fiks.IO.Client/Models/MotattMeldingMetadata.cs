using System;

namespace KS.Fiks.IO.Client.Models
{
    public class MotattMeldingMetadata : MeldingBase
    {
        public MotattMeldingMetadata(Guid meldingId, string meldingType, Guid mottakerKontoId, Guid avsenderKontoId, Guid? svarPaMelding, TimeSpan ttl)
        {
            MeldingId = meldingId;
            MeldingType = meldingType;
            MottakerKontoId = mottakerKontoId;
            AvsenderKontoId = avsenderKontoId;
            SvarPaMelding = svarPaMelding;
            Ttl = ttl;
        }

        public MotattMeldingMetadata(MotattMeldingMetadata metadata)
            : base(metadata)
        {
            SvarPaMelding = metadata.SvarPaMelding;
        }
    }
}