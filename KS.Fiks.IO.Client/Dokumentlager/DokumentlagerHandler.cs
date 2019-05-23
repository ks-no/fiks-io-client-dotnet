using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;
using KS.Fiks.IO.Send.Client.Authentication;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Dokumentlager
{
    internal class DokumentlagerHandler : IDokumentlagerHandler
    {
        private readonly DokumentlagerConfiguration _dokumentlagerConfiguration;

        private readonly HttpClient _httpClient;

        private readonly IAuthenticationStrategy _authenticationStrategy;

        public DokumentlagerHandler(
            DokumentlagerConfiguration dokumentlagerConfiguration,
            IntegrationConfiguration integrationConfiguration,
            IMaskinportenClient maskinportenClient,
            IAuthenticationStrategy authenticationStrategy = null,
            HttpClient httpClient = null)
        {
            _dokumentlagerConfiguration = dokumentlagerConfiguration;
            _authenticationStrategy = authenticationStrategy ??
                                      new IntegrasjonAuthenticationStrategy(maskinportenClient, integrationConfiguration.IntegrationId, integrationConfiguration.IntegrationPassword);
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<Stream> Download(Guid messageId)
        {
            var response = await _httpClient.SendAsync(await CreateRequestMessage(messageId).ConfigureAwait(false)).ConfigureAwait(false);
            ThrowIfContentIsEmpty(response, messageId);
            var content = response.Content;
            var contentBytes = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return new MemoryStream(contentBytes);
            //return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private async Task<HttpRequestMessage> CreateRequestMessage(Guid messageId)
        {
            var uri = new UriBuilder(_dokumentlagerConfiguration.Scheme, _dokumentlagerConfiguration.Host, _dokumentlagerConfiguration.Port, $"{_dokumentlagerConfiguration.DownloadPath}/{messageId}").Uri;

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var keyValuePair in await _authenticationStrategy
                                               .GetAuthorizationHeaders().ConfigureAwait(false))
            {
                requestMessage.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return requestMessage;
        }

        private void ThrowIfContentIsEmpty(HttpResponseMessage responseMessage, Guid messageId)
        {
            if (responseMessage.Content.Headers.ContentLength < 1)
            {
                throw new FiksIODokumentlagerResponseException(
                    $"Response content for message ({messageId.ToString()}) is empty.");
            }
        }
    }
}