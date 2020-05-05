using System;
using System.Collections.Generic;

namespace KS.Fiks.IO.Client.Models
{
    public interface IMelding
    {
        Guid MeldingId { get; }

        string MeldingType { get; }

        Guid AvsenderKontoId { get; }

        Guid MottakerKontoId { get; }

        TimeSpan Ttl { get; }
        
        Dictionary<string, string> Headere { get; }
        
        public bool Resendt { get; }
    }
}
