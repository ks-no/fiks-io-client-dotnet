using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Models
{
    public class AccountResponse
    {
        [JsonProperty("fiksOrgId")]
        public Guid OrgId { get; set; }

        [JsonProperty("fiksOrgNavn")]
        public string OrgName { get; set; }

        [JsonProperty("kontoId")]
        public Guid AccountId { get; set; }

        [JsonProperty("kontoNavn")]
        public string AccountName { get; set; }

        [JsonProperty("status")]
        public AccountResponseStatus Status { get; set; }
    }
}