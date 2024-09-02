using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Catalog
{
    internal class CatalogHandler : ICatalogHandler
    {
        private const string LookupEndpoint = "lookup";

        private const string PublicKeyEndpoint = "offentligNokkel";

        private const string AccountsEndpoint = "kontoer";

        private const string StatusEndpoint = "status";

        private const string IdentifyerQueryName = "identifikator";

        private const string MessageProtocolQueryName = "meldingProtokoll";

        private const string AccessLevelQueryName = "sikkerhetsniva";

        private readonly HttpClient _httpClient;

        private readonly KatalogConfiguration _katalogConfiguration;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private readonly IMaskinportenClient _maskinportenClient;

        public CatalogHandler(
            KatalogConfiguration katalogConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            IMaskinportenClient maskinportenClient,
            HttpClient httpClient = null)
        {
            _katalogConfiguration = katalogConfiguration;
            _integrasjonConfiguration = integrasjonConfiguration;
            _maskinportenClient = maskinportenClient;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<Konto> Lookup(LookupRequest request)
        {
            var requestUri = CreateLookupUri(request);
            var responseAsAccount = await GetAsModel<KatalogKonto>(requestUri).ConfigureAwait(false);
            return Konto.FromKatalogModel(responseAsAccount);
        }

        public async Task<Konto> GetKonto(Guid kontoId)
        {
            var requestUri = CreateGetKontoUri(kontoId);
            var responseAsAccount = await GetAsModel<KatalogKonto>(requestUri).ConfigureAwait(false);
            return Konto.FromKatalogModel(responseAsAccount);
        }

        public async Task<Status> GetStatus(Guid kontoId)
        {
            var requestUri = CreateGetKontoStatusUri(kontoId);
            var responseAsAccount = await GetAsModel<KontoSvarStatus>(requestUri).ConfigureAwait(false);
            return Status.FromKatalogModel(responseAsAccount);
        }

        public async Task<X509Certificate> GetPublicKey(Guid receiverAccountId)
        {
            var requestUri = CreatePublicKeyUri(receiverAccountId);
            var responseAsPublicKeyModel = await GetAsModel<KontoOffentligNokkel>(requestUri, authenticated: false).ConfigureAwait(false);
            return X509CertificateReader.ExtractCertificate(responseAsPublicKeyModel.Nokkel);
        }

        private static async Task ThrowIfResponseIsInvalid(HttpResponseMessage response, Uri requestUri)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIOUnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {requestUri}. Content: {content}.");
            }
        }

        private Uri CreateLookupUri(LookupRequest request)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{LookupEndpoint}";
            var query = $"?{IdentifyerQueryName}={request.Identifikator}&" +
                        $"{MessageProtocolQueryName}={request.Meldingsprotokoll}&" +
                        $"{AccessLevelQueryName}={request.Sikkerhetsniva}";

            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath,
                    query)
                .Uri;
        }

        private Uri CreateGetKontoUri(Guid kontoId)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{kontoId.ToString()}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private Uri CreateGetKontoStatusUri(Guid kontoId)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{kontoId.ToString()}/{StatusEndpoint}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private Uri CreatePublicKeyUri(Guid receiverAccountId)
        {
            var servicePath =
                $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{receiverAccountId.ToString()}/{PublicKeyEndpoint}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private async Task<T> GetAsModel<T>(Uri requestUri, bool authenticated = true)
        {
            var accessToken = await _maskinportenClient
                                    .GetAccessToken(_integrasjonConfiguration.Scope).ConfigureAwait(false);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                if (authenticated)
                {
                    requestMessage.Headers.Add(
                        "integrasjonId",
                        _integrasjonConfiguration.IntegrasjonId.ToString());

                    requestMessage.Headers.Add(
                        "integrasjonPassord",
                        _integrasjonConfiguration.IntegrasjonPassord);
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken.Token);
                }

                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                await ThrowIfResponseIsInvalid(response, requestUri).ConfigureAwait(false);
                var responseAsJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(responseAsJsonString);
            }
        }
    }
}