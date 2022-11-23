using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class KontoConfiguration
    {
        public KontoConfiguration(Guid kontoId, string privatNokkel)
        {
            KontoId = kontoId;
            PrivatNokkel = privatNokkel;
        }

        public Guid KontoId { get; }

        /**
        * Privat nøkkel som matcher det offentlige sertifikatet som er spesifisert for kontoen i fiks-konfigurasjon. Benyttes for å dekryptere inkommende meldinger.
        */
        public string PrivatNokkel { get; }
    }
}