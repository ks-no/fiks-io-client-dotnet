using Newtonsoft.Json;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MaskinportenResponse
    {
        [JsonProperty("expires_in")] public int ExpiresIn { get; private set; }

        [JsonProperty("access_token")] public string AccessToken { get; private set; }
    }
}