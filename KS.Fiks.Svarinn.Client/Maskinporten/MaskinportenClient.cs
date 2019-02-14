using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private MaskinportenClientProperties _properties;
        private HttpClient _httpClient;
        private Dictionary<string, string> _accessTokenCache;

        public MaskinportenClient(MaskinportenClientProperties properties, HttpClient httpClient = null)
        {
            _properties = properties;
            _httpClient = httpClient ?? new HttpClient();
            _accessTokenCache = new Dictionary<string, string>();
        }

        public async Task<string> GetAccessToken(IEnumerable<string> scopes)
        {
            var scopeKey = ScopesToKey(scopes);
            if (HasValidCachedAccessToken(scopeKey))
            {
                return _accessTokenCache[scopeKey];
            }

            var accessToken = await GetNewAccessToken();
            StoreAccessTokenInCache(accessToken, scopeKey);

            return accessToken;
        }


        private string ScopesToKey(IEnumerable<string> scopes)
        {
            var keyBuffer = new StringBuilder();
            foreach (var scope in scopes)
            {
                keyBuffer.Append(scope);
            }

            return keyBuffer.ToString();
        }

        private bool HasValidCachedAccessToken(string scopeKey)
        {
            return _accessTokenCache.ContainsKey(scopeKey);
        }

        private async Task<string> GetNewAccessToken()
        {
            var response = await _httpClient.GetAsync(_properties.TokenEndpoint);
            var maskinportenResponse = await ReadResponse(response);
            return maskinportenResponse.AccessToken;
        }

        private async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }

        private void StoreAccessTokenInCache(string accessToken, string scopeKey)
        {
            _accessTokenCache.Add(scopeKey, accessToken);
        }
    }
}