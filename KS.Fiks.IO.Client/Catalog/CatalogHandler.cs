using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Client.Catalog
{
    internal class CatalogHandler : ICatalogHandler
    {
        private const string LookupEndpoint = "lookup";

        private const string PublicKeyEndpoint = "offentligNokkel";

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
            _catalogConfiguration = catalogConfiguration;
            _integrationConfiguration = integrationConfiguration;
            _maskinportenClient = maskinportenClient;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<Account> Lookup(LookupRequest request)
        {
            var requestUri = CreateLookupUri(request);
            var responseAsAccount = await GetAsModel<AccountResponse>(requestUri).ConfigureAwait(false);
            return Account.FromAccountResponse(responseAsAccount);
        }

        public async Task<string> GetPublicKey(Guid receiverAccountId)
        {
            var requestUri = CreatePublicKeyUri(receiverAccountId);
            var responseAsPublicKeyModel = await GetAsModel<AccountPublicKey>(requestUri).ConfigureAwait(false);
            return responseAsPublicKeyModel.Key;
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

        private Uri CreatePublicKeyUri(Guid receiverAccountId)
        {
            var servicePath = $"{_catalogConfiguration.Path}/{receiverAccountId.ToString()}/{PublicKeyEndpoint}";
            return new UriBuilder(
                    _catalogConfiguration.Scheme,
                    _catalogConfiguration.Host,
                    _catalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private async Task<T> GetAsModel<T>(Uri requestUri)
        {
            await SetAuthorizationHeader().ConfigureAwait(false);
            var response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);
            await ThrowIfResponseIsInvalid(response, requestUri).ConfigureAwait(false);
            var responseAsJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseAsJsonString);
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
                throw new FiksIOUnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {requestUri}. Content: {content}.");
            }
        }
    }
}