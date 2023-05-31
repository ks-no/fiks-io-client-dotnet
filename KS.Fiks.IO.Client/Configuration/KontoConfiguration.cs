using System;
using System.Collections.Generic;
using System.Linq;

namespace KS.Fiks.IO.Client.Configuration
{
    public class KontoConfiguration
    {
        public KontoConfiguration(Guid kontoId, string privatNokkel)
        {
            KontoId = kontoId;
            PrivatNokler = new List<string> {privatNokkel};
        }

        public KontoConfiguration(Guid kontoId, IEnumerable<string> privatNokler)
        {
            KontoId = kontoId;
            if (privatNokler == null)
            {
                throw new ArgumentNullException(nameof(privatNokler));
            }

            PrivatNokler = privatNokler.ToList();

            if (!PrivatNokler.Any())
            {
                throw new ArgumentNullException(nameof(privatNokler), "Must provide atleast one private key");
            }
        }

        public Guid KontoId { get; }

        /**
        * Privat nøkkel som matcher den offentlige nøkkelen som er spesifisert for kontoen i fiks-konfigurasjon. Benyttes for å dekryptere innkommende meldinger.
        *
        * For å støtte nøkkelrotasjon er det mulig å legge til flere private nøkler.
        */
        public List<string> PrivatNokler { get; }
    }
}