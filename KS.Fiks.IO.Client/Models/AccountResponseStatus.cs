using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models
{
    public class AccountResponseStatus
    {
        [JsonProperty("gyldigAvsender")]
        public bool ValidSender { get; set; }

        [JsonProperty("gyldigMottaker")]
        public bool ValidReceiver { get; set; }

        [JsonProperty("melding")]
        public string Message { get; set; }
    }
}