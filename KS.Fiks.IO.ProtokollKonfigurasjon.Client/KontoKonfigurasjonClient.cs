using System;
using System.Net.Http;
using System.Threading.Tasks;
using KS.Fiks.IO.ProtokollKonfigurasjon.Client.Generated;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    public static class KontoKonfigurasjonClient
    {
        /// <summary>
        /// Creates a client for the Fiks protokoll-konfigurasjon API.
        /// </summary>
        /// <param name="hostUrl">Base URL of the protokoll-konfigurasjon API.</param>
        /// <param name="integrasjonId">Integrasjon id used for authentication.</param>
        /// <param name="integrasjonPassord">Integrasjon password used for authentication.</param>
        /// <param name="maskinportenTokenSupplier">
        /// Supplies a Maskinporten access token. This is invoked on every outgoing HTTP request made by the
        /// returned client, so it should return a cached/memoized token (only fetching a new one from
        /// Maskinporten when the cached token is missing or about to expire) rather than requesting a new
        /// token on every call. <c>Ks.Fiks.Maskinporten.Client.MaskinportenClient.GetAccessToken</c> already
        /// caches internally and is safe to pass directly.
        /// </param>
        public static IProtokollKonfigurasjonClient CreateClient(
            string hostUrl,
            Guid integrasjonId,
            string integrasjonPassord,
            Func<Task<string>> maskinportenTokenSupplier)
        {
            var httpClient = new HttpClient(new FiksIntegrasjonHttpMessageHandler(integrasjonId, integrasjonPassord, maskinportenTokenSupplier));
            return new ProtokollKonfigurasjonClient(httpClient) { BaseUrl = hostUrl };
        }
    }
}
