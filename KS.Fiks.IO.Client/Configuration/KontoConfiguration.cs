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
        * Privat nøkkel som matcher den offentlige nøkkelen som er spesifisert for kontoen i fiks-konfigurasjon. Benyttes for å dekryptere innkommende meldinger.
        */
        public string PrivatNokkel { get; }
    }
}