using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private const string GRANT_TYPE = "urn:ietf:params:oauth:grant-type:jwt-bearer";


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
            SetRequestHeaders();
            var response = await _httpClient.PostAsync(_properties.TokenEndpoint, CreateRequestContent());
            var maskinportenResponse = await ReadResponse(response);
            return maskinportenResponse.AccessToken;
        }

        private void SetRequestHeaders()
        {
            _httpClient.DefaultRequestHeaders.Add("Charset", "utf-8");
        }

        private StringContent CreateRequestContent()
        {
            var request = new MaskinportenRequest()
            {
                GrantType = GRANT_TYPE,
                Assertion = "something"
            };
            var requestAsJson = JsonConvert.SerializeObject(request);
            return new StringContent(requestAsJson, Encoding.UTF8, "application/x-www-form-urlencoded");
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