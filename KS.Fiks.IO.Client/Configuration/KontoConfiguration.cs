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

        public string PrivatNokkel { get; }
    }
}