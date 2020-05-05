using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Models
{
    public abstract class MeldingBase : IMelding
    {
        protected MeldingBase()
        {
        }

        protected MeldingBase(Guid meldingId,
            string meldingType,
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            TimeSpan ttl,
            bool resendt = false,
            Dictionary<string, string> headere = null,
            Guid? svarPaMelding = null)
        {
            MeldingId = meldingId;
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
            MeldingType = melding.MeldingType;
            AvsenderKontoId = melding.AvsenderKontoId;
            MottakerKontoId = melding.MottakerKontoId;
            Ttl = melding.Ttl;
            Headere = melding.Headere;
            Resendt = melding.Resendt;
        }

        public Guid MeldingId { get; protected set; }

        public string MeldingType { get; protected set; }

        public Guid AvsenderKontoId { get; protected set; }

        public Guid MottakerKontoId { get; protected set; }

        public TimeSpan Ttl { get; protected set; }
        
        public Dictionary<string, string> Headere { get; protected set; }

        public Guid? SvarPaMelding { get; protected set; }
        
        public Boolean Resendt { get; protected set; }
    }
}