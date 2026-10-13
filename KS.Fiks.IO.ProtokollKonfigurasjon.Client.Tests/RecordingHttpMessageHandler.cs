using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client.Tests
{
    /// <summary>
    /// A fake innermost <see cref="HttpMessageHandler"/> that records the last request it received
    /// and how many times it was invoked, without making any real network call.
    /// </summary>
    internal class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
