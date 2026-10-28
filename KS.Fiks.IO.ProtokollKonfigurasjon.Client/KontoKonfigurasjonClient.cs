using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    public static class KontoKonfigurasjonClient
    {
        /// <summary>
        /// Creates a client for the Fiks protokoll-konfigurasjon API.
        /// </summary>
        /// <param name="hostUrl">
        /// Base URL of the protokoll-konfigurasjon API, e.g. <c>https://api.fiks.ks.no</c> or
        /// <c>https://api.fiks.test.ks.no</c>. Must be an absolute <c>https</c> URL: the integrasjon
        /// password and the Maskinporten token are sent to this host on every request, so a plaintext
        /// <c>http</c> URL would expose both on the wire. The returned client only ever sends those
        /// credentials to this URL's origin, even if its <c>BaseUrl</c> is changed afterwards.
        /// </param>
        /// <param name="integrasjonId">Integrasjon id used for authentication.</param>
        /// <param name="integrasjonPassord">Integrasjon password used for authentication.</param>
        /// <param name="maskinportenTokenSupplier">
        /// Supplies a Maskinporten access token. This is invoked on every outgoing HTTP request made by the
        /// returned client, so it should return a cached/memoized token (only fetching a new one from
        /// Maskinporten when the cached token is missing or about to expire) rather than requesting a new
        /// token on every call. <c>Ks.Fiks.Maskinporten.Client.MaskinportenClient.GetAccessToken</c> already
        /// caches internally, but takes a scope and returns a token object, so wrap it in a delegate that
        /// supplies the scope and returns the token string, e.g.
        /// <c>async () =&gt; (await maskinportenClient.GetAccessToken(scope)).Token</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="hostUrl"/> is not an absolute URL, or does not use <c>https</c>.
        /// </exception>
        public static IProtokollKonfigurasjonClient CreateClient(
            string hostUrl,
            Guid integrasjonId,
            string integrasjonPassord,
            Func<Task<string>> maskinportenTokenSupplier)
        {
            var hostUri = ValidateHostUrl(hostUrl);

            var httpClient = new HttpClient(new FiksIntegrasjonHttpMessageHandler(hostUri, integrasjonId, integrasjonPassord, maskinportenTokenSupplier));
            return new ProtokollKonfigurasjonClient(httpClient, ownsHttpClient: true) { BaseUrl = hostUrl };
        }

        // Every request carries the integrasjon password and a Maskinporten bearer token, so a hostUrl
        // that is relative (and therefore resolved against something else) or plaintext http would hand
        // those credentials out. Fail fast on both instead of discovering it on the wire. The validated
        // origin is then pinned in the handler, since BaseUrl stays settable on the generated client.
        private static Uri ValidateHostUrl(string hostUrl)
        {
            if (!Uri.TryCreate(hostUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException(
                    "hostUrl must be an absolute URL, e.g. https://api.fiks.ks.no.",
                    nameof(hostUrl));
            }

            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException(
                    $"hostUrl must use https, but was '{uri.Scheme}'. The integrasjon password and the " +
                    "Maskinporten token are sent to hostUrl on every request.",
                    nameof(hostUrl));
            }

            return uri;
        }
    }
}
