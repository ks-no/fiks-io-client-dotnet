using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models.Feilmelding
{
    public class Ugyldigforespørsel
    {
        [JsonProperty("errorId")]
        public string ErrorId { get; set; }

        [JsonProperty("feilmelding")]
        public string Feilmelding { get; set; }

        [JsonProperty("referanseMeldingId")]
        public Guid ReferanseMeldingId { get; set; }
    }
}