using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Exceptions;

namespace KS.Fiks.IO.Client.Dokumentlager
{
    internal class DokumentlagerHandler : IDokumentlagerHandler
    {
        private readonly DokumentlagerConfiguration _configuration;

        private readonly HttpClient _httpClient;

        public DokumentlagerHandler(DokumentlagerConfiguration configuration, HttpClient httpClient = null)
        {
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<Stream> Download(Guid messageId)
        {
            var response = await _httpClient.SendAsync(CreateRequestMessage(messageId)).ConfigureAwait(false);
            ThrowIfContentIsEmpty(response, messageId);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private HttpRequestMessage CreateRequestMessage(Guid messageId)
        {
            var uri = new UriBuilder(_configuration.Scheme, _configuration.Host, _configuration.Port,$"{_configuration.DownloadPath}/{messageId}").Uri;

            return new HttpRequestMessage(HttpMethod.Get, uri);
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