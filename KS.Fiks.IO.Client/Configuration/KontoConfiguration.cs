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

        public KontoConfiguration(Guid kontoId, string privatNokkel, string offentligNokkel)
        {
            KontoId = kontoId;
            PrivatNokler = new List<string> {privatNokkel};
            OffentligNokkel = offentligNokkel;
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
                throw new ArgumentException("Must provide at least one private key.", nameof(privatNokler));
            }
        }

        public KontoConfiguration(Guid kontoId, IEnumerable<string> privatNokler, string offentligNokkel)
        {
            KontoId = kontoId;
            if (privatNokler == null)
            {
                throw new ArgumentNullException(nameof(privatNokler));
            }

            PrivatNokler = privatNokler.ToList();

            if (!PrivatNokler.Any())
            {
                throw new ArgumentException("Must provide at least one private key.", nameof(privatNokler));
            }

            OffentligNokkel = offentligNokkel;
        }

        public Guid KontoId { get; }

        /// <summary>
        /// Privat nøkkel som matcher den offentlige nøkkelen som er spesifisert for kontoen i fiks-konfigurasjon. Benyttes for å dekryptere innkommende meldinger.
        /// For å støtte nøkkelrotasjon er det mulig å legge til flere private nøkler.
        /// </summary>
        public List<string> PrivatNokler { get; }

        /// <summary>
        /// Valgfri offentlig nøkkel (PEM-kodet X.509-sertifikat) for kontoen. Når satt vil klienten automatisk laste opp
        /// nøkkelen til Fiks-IO katalog ved oppstart dersom nøkkelen mangler eller er utdatert.
        /// </summary>
        public string OffentligNokkel { get; }
    }
}