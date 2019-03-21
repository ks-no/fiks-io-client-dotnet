using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;
using UnexpectedResponseException = KS.Fiks.IO.Client.Exceptions.UnexpectedResponseException;

namespace KS.Fiks.IO.Client.Catalog
{
    public class CatalogHandler : ICatalogHandler
    {
        private const string LookupEndpoint = "lookup";

        private const string AuthenticationScope = "ks";

        private const string IdentifyerQueryName = "identifikator";

        private const string MessageTypeQueryName = "meldingType";

        private const string AccessLevelQueryName = "sikkerhetsniva";

        private readonly HttpClient _httpClient;

        private readonly CatalogConfiguration _catalogConfiguration;
        private readonly FiksIntegrationConfiguration _integrationConfiguration;
        private readonly IMaskinportenClient _maskinportenClient;

        public CatalogHandler(
            CatalogConfiguration catalogConfiguration,
            FiksIntegrationConfiguration integrationConfiguration,
            IMaskinportenClient maskinportenClient,
            HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _catalogConfiguration = catalogConfiguration;
            _integrationConfiguration = integrationConfiguration;
            _maskinportenClient = maskinportenClient;
        }

        public async Task<Account> Lookup(LookupRequest request)
        {
            await SetAuthorizationHeader().ConfigureAwait(false);
            var requestUri = CreateLookupUri(request);
            var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            await ThrowIfResponseIsInvalid(response, requestUri).ConfigureAwait(false);
            var responseAsJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseAsAccount = JsonConvert.DeserializeObject<AccountResponse>(responseAsJsonString);
            return Account.FromAccountResponse(responseAsAccount);
        }

        public Task<string> GetPublicKey(Guid receiverAccountId)
        {
            throw new NotImplementedException();
        }

        private Uri CreateLookupUri(LookupRequest request)
        {
            var servicePath = $"{_catalogConfiguration.Path}/{LookupEndpoint}";
            var query = $"?{IdentifyerQueryName}={request.Identifier}&" +
                        $"{MessageTypeQueryName}={request.MessageType}&" +
                        $"{AccessLevelQueryName}={request.AccessLevel}";

            return new UriBuilder(
                    _catalogConfiguration.Scheme,
                    _catalogConfiguration.Host,
                    _catalogConfiguration.Port,
                    servicePath,
                    query)
                .Uri;
        }

        private async Task SetAuthorizationHeader()
        {
            var accessToken = await _maskinportenClient.GetAccessToken(AuthenticationScope).ConfigureAwait(false);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("integrasjonId", _integrationConfiguration.IntegrastionId.ToString());
            _httpClient.DefaultRequestHeaders.Add("integrasjonPassord", _integrationConfiguration.IntegrationPassword);
        }

        private async Task ThrowIfResponseIsInvalid(HttpResponseMessage response, Uri requestUri)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new UnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {requestUri}. Content: {content}.");
            }
        }
    }
}