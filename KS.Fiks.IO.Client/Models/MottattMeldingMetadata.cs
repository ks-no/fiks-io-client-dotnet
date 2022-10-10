using System;
using System.Collections.Generic;

namespace KS.Fiks.IO.Client.Models
{
    public class MottattMeldingMetadata : MeldingBase
    {
        public MottattMeldingMetadata(Guid meldingId, string meldingType, Guid mottakerKontoId, Guid avsenderKontoId, Guid? svarPaMelding, TimeSpan ttl, Dictionary<string, string> headere, bool resendt = false)
        {
            MeldingId = meldingId;
            MeldingType = meldingType;
            MottakerKontoId = mottakerKontoId;
            AvsenderKontoId = avsenderKontoId;
            SvarPaMelding = svarPaMelding;
            Ttl = ttl;
            Headere = headere;
            Resendt = resendt;
        }

        public MottattMeldingMetadata(MottattMeldingMetadata metadata)
            : base(metadata)
        {
            SvarPaMelding = metadata.SvarPaMelding;
            Resendt = metadata.Resendt;
        }
    }
}