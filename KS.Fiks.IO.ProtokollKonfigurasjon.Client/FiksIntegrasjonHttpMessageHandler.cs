using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    internal class FiksIntegrasjonHttpMessageHandler : DelegatingHandler
    {
        private const string IntegrasjonIdHeader = "IntegrasjonId";
        private const string IntegrasjonPassordHeader = "IntegrasjonPassord";
        private const string RequestIdHeader = "requestId";

        private readonly Guid _integrasjonId;
        private readonly string _integrasjonPassord;
        private readonly Func<Task<string>> _maskinportenTokenSupplier;

        public FiksIntegrasjonHttpMessageHandler(Guid integrasjonId, string integrasjonPassord, Func<Task<string>> maskinportenTokenSupplier)
            : base(new HttpClientHandler())
        {
            _integrasjonId = integrasjonId;
            _integrasjonPassord = integrasjonPassord;
            _maskinportenTokenSupplier = maskinportenTokenSupplier;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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
    }
}
