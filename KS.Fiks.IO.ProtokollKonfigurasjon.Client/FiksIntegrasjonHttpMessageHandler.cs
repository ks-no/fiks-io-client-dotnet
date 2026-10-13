using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    /// <summary>
    /// Adds the Fiks integrasjon authentication headers (including the integrasjon password and a bearer
    /// token) to every outgoing request, as long as that request is going to the origin the handler was
    /// pinned to.
    /// </summary>
    /// <remarks>
    /// This handler must remain the innermost <see cref="DelegatingHandler"/> in the chain (i.e. closest to
    /// the wire, directly wrapping the transport handler), which is guaranteed today since it is only ever
    /// constructed by <see cref="KontoKonfigurasjonClient.CreateClient"/> and is not part of the public API.
    /// Do not insert any additional handler between this handler and the transport handler, and do not add a
    /// logging/telemetry handler outside this one that logs <see cref="HttpRequestMessage.Headers"/> after
    /// the request has been forwarded down the chain: doing so would capture the integrasjon password and
    /// bearer token headers added below.
    /// </remarks>
    internal class FiksIntegrasjonHttpMessageHandler : DelegatingHandler
    {
        private const string IntegrasjonIdHeader = "IntegrasjonId";
        private const string IntegrasjonPassordHeader = "IntegrasjonPassord";
        private const string RequestIdHeader = "requestId";

        private readonly string _pinnedOrigin;
        private readonly Guid _integrasjonId;
        private readonly string _integrasjonPassord;
        private readonly Func<Task<string>> _maskinportenTokenSupplier;

        /// <param name="hostUrl">
        /// The absolute https URL the credentials belong to. Only its origin (scheme, host and port) is kept:
        /// requests to any other origin are refused rather than authenticated.
        /// </param>
        public FiksIntegrasjonHttpMessageHandler(Uri hostUrl, Guid integrasjonId, string integrasjonPassord, Func<Task<string>> maskinportenTokenSupplier)
            : base(new HttpClientHandler { AllowAutoRedirect = false })
        {
            if (hostUrl == null)
            {
                throw new ArgumentNullException(nameof(hostUrl));
            }

            if (!hostUrl.IsAbsoluteUri || hostUrl.Scheme != Uri.UriSchemeHttps)
            {
                throw new ArgumentException("hostUrl must be an absolute https URL.", nameof(hostUrl));
            }

            _pinnedOrigin = hostUrl.GetLeftPart(UriPartial.Authority);
            _integrasjonId = integrasjonId;
            _integrasjonPassord = integrasjonPassord;
            _maskinportenTokenSupplier = maskinportenTokenSupplier;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // The generated client exposes a publicly settable BaseUrl, so the check in
            // KontoKonfigurasjonClient.CreateClient only holds at construction time. Re-check here, where
            // the credentials are actually attached to the request, and against the pinned origin rather
            // than just the scheme: otherwise the password and the token follow BaseUrl to any https host.
            if (!IsPinnedOrigin(request.RequestUri))
            {
                throw new InvalidOperationException(
                    $"Refusing to send the integrasjon password and the Maskinporten token to '{request.RequestUri}': " +
                    $"these credentials are pinned to '{_pinnedOrigin}'.");
            }

            // Remove any headers already present before (re-)adding them, since the same HttpRequestMessage
            // instance may be sent more than once (e.g. by retry policies), and Headers.Add throws on duplicates.
            request.Headers.Remove(IntegrasjonIdHeader);
            request.Headers.Add(IntegrasjonIdHeader, _integrasjonId.ToString());

            request.Headers.Remove(IntegrasjonPassordHeader);
            request.Headers.Add(IntegrasjonPassordHeader, _integrasjonPassord);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _maskinportenTokenSupplier().ConfigureAwait(false));

            request.Headers.Remove(RequestIdHeader);
            request.Headers.Add(RequestIdHeader, Guid.NewGuid().ToString());

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // Comparing the whole origin (scheme, host and port) also covers the scheme: the pinned origin is
        // always https, so anything plaintext fails the comparison.
        private bool IsPinnedOrigin(Uri? requestUri)
        {
            return requestUri != null
                   && requestUri.IsAbsoluteUri
                   && string.Equals(requestUri.GetLeftPart(UriPartial.Authority), _pinnedOrigin, StringComparison.OrdinalIgnoreCase);
        }
    }
}
