using System;

namespace KS.Fiks.IO.Client.Models
{
    public class MottattMeldingMetadata : MeldingBase
    {
        public MottattMeldingMetadata(Guid meldingId, string meldingType, Guid mottakerKontoId, Guid avsenderKontoId, Guid? svarPaMelding, TimeSpan ttl)
        {
            MeldingId = meldingId;
            MeldingType = meldingType;
            MottakerKontoId = mottakerKontoId;
            AvsenderKontoId = avsenderKontoId;
            SvarPaMelding = svarPaMelding;
            Ttl = ttl;
        }

        public MottattMeldingMetadata(MottattMeldingMetadata metadata)
            : base(metadata)
        {
            SvarPaMelding = metadata.SvarPaMelding;
        }
    }
}