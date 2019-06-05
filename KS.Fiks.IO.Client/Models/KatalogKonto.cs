using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models
{
    public class KatalogKonto
    {
        [JsonProperty("fiksOrgId")]
        public Guid FiksOrgId { get; set; }

        [JsonProperty("fiksOrgNavn")]
        public string FiksOrgNavn { get; set; }

        [JsonProperty("kontoId")]
        public Guid KontoId { get; set; }

        [JsonProperty("kontoNavn")]
        public string KontoNavn { get; set; }

        [JsonProperty("status")]
        public KontoSvarStatus Status { get; set; }
    }
}