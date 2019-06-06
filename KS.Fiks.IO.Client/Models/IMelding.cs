using System;

namespace KS.Fiks.IO.Client.Models
{
    public interface IMelding
    {
        Guid MeldingId { get; }

        string MeldingType { get; }

        Guid AvsenderKontoId { get; }

        Guid MottakerKontoId { get; }

        TimeSpan Ttl { get; }
    }
}
