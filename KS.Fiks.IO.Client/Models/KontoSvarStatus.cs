using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models
{
    public class KontoSvarStatus
    {
        [JsonProperty("gyldigAvsender")]
        public bool GyldigAvsender { get; set; }

        [JsonProperty("gyldigMottaker")]
        public bool GyldigMottaker { get; set; }

        [JsonProperty("antallKonsumenter")]
        public long AntallKonsumenter { get; set; }

        [JsonProperty("melding")]
        public string Melding { get; set; }
    }
}