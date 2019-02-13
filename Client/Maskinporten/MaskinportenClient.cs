using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private MaskinportenClientProperties _properties;
        private HttpClient _httpClient;
        
        public MaskinportenClient(MaskinportenClientProperties properties, HttpClient httpClient = null)
        {
            _properties = properties;
            _httpClient = httpClient ?? new HttpClient();
        }
        public async Task<string> GetAccessToken(ICollection<string> scopes)
        {
            await _httpClient.GetAsync("https://api.fiks.ks.no");
            return "dummy";
        }
    }
}