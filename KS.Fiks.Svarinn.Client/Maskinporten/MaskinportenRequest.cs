using Newtonsoft.Json;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    [JsonObject(MemberSerialization.OptIn)]

    public class MaskinportenRequest
    {
        
            [JsonProperty("grant_type")] public string GrantType { get; set; }
            [JsonProperty("assertion")] public string Assertion { get; set; }
    }
}