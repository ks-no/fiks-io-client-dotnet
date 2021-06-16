using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models.Feilmelding
{
    public class ServerFeil
    {
        [JsonProperty("errorId")]
        public string ErrorId { get; set; }

        [JsonProperty("feilmelding")]
        public string Feilmelding { get; set; }

        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }
    }
}