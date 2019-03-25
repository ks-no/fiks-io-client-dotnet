using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models
{
    public class AccountPublicKey
    {
        [JsonProperty("nokkel")]
        public string Key { get; set; }

        [JsonProperty("validFrom")]
        public DateTime ValidFrom { get; set; }

        [JsonProperty("validTo")]
        public DateTime ValidTo { get; set; }

        [JsonProperty("serial")]
        public string Serial { get; set; }

        [JsonProperty("subjectDN")]
        public string SubjectDistinguishedName { get; set; }

        [JsonProperty("issuerDN")]
        public string IssuerDistinguishedName { get; set; }
    }
}