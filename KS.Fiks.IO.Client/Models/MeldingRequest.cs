using System;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Client.Models
{
    public class MeldingRequest : MeldingBase
    {
        private const int DefaultTtlInDays = 2;

        public MeldingRequest(
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            string meldingType,
            TimeSpan? ttl = null,
            Guid? svarPaMelding = null)
        : base(Guid.Empty, meldingType, avsenderKontoId, mottakerKontoId, ttl ?? TimeSpan.FromDays(DefaultTtlInDays), svarPaMelding)
        {
        }

        public MeldingSpesifikasjonApiModel ToApiModel()
        {
            return new MeldingSpesifikasjonApiModel(
                AvsenderKontoId,
                MottakerKontoId,
                MeldingType,
                (long)Ttl.TotalMilliseconds,
                SvarPaMelding);
        }
    }
}