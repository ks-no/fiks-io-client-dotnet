using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client.Tests
{
    public class FiksIntegrasjonHttpMessageHandlerTests
    {
        private const string IntegrasjonIdHeader = "IntegrasjonId";
        private const string IntegrasjonPassordHeader = "IntegrasjonPassord";
        private const string RequestIdHeader = "requestId";

        [Fact]
        public async Task SendAsync_SetsIntegrasjonIdIntegrasjonPassordAndBearerTokenHeaders()
        {
            var integrasjonId = Guid.NewGuid();
            var integrasjonPassord = "some-password";
            var token = "some-maskinporten-token";
            var recordingHandler = new RecordingHttpMessageHandler();
            var sut = new FiksIntegrasjonHttpMessageHandler(integrasjonId, integrasjonPassord, () => Task.FromResult(token))
            {
                InnerHandler = recordingHandler
            };

            using var invoker = new HttpMessageInvoker(sut);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api");

            await invoker.SendAsync(request, CancellationToken.None);

            recordingHandler.CallCount.ShouldBe(1);
            recordingHandler.LastRequest.ShouldNotBeNull();
            recordingHandler.LastRequest!.Headers.GetValues(IntegrasjonIdHeader).Single().ShouldBe(integrasjonId.ToString());
            recordingHandler.LastRequest.Headers.GetValues(IntegrasjonPassordHeader).Single().ShouldBe(integrasjonPassord);
            recordingHandler.LastRequest.Headers.Authorization.ShouldNotBeNull();
            recordingHandler.LastRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
            recordingHandler.LastRequest.Headers.Authorization.Parameter.ShouldBe(token);
        }

        [Fact]
        public async Task SendAsync_SetsARequestIdHeader()
        {
            var recordingHandler = new RecordingHttpMessageHandler();
            var sut = new FiksIntegrasjonHttpMessageHandler(Guid.NewGuid(), "password", () => Task.FromResult("token"))
            {
                InnerHandler = recordingHandler
            };

            using var invoker = new HttpMessageInvoker(sut);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api");

            await invoker.SendAsync(request, CancellationToken.None);

            var requestId = recordingHandler.LastRequest!.Headers.GetValues(RequestIdHeader).Single();
            Guid.TryParse(requestId, out _).ShouldBeTrue();
        }

        [Fact]
        public async Task SendAsync_InvokesTokenSupplierOnEveryCall()
        {
            var tokenCallCount = 0;
            var recordingHandler = new RecordingHttpMessageHandler();
            var sut = new FiksIntegrasjonHttpMessageHandler(Guid.NewGuid(), "password", () =>
            {
                tokenCallCount++;
                return Task.FromResult($"token-{tokenCallCount}");
            })
            {
                InnerHandler = recordingHandler
            };

            using var invoker = new HttpMessageInvoker(sut);
            using var firstRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api");
            using var secondRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api");

            await invoker.SendAsync(firstRequest, CancellationToken.None);
            await invoker.SendAsync(secondRequest, CancellationToken.None);

            tokenCallCount.ShouldBe(2);
        }

        [Fact]
        public async Task SendAsync_CanResendTheSameRequestMessageWithoutThrowing()
        {
            // Regression test: HttpRequestMessage.Headers.Add throws on a header that is already
            // present, which would happen if the same HttpRequestMessage instance is sent more than
            // once (e.g. by a retry policy) unless the handler removes its own headers before
            // re-adding them.
            var recordingHandler = new RecordingHttpMessageHandler();
            var sut = new FiksIntegrasjonHttpMessageHandler(Guid.NewGuid(), "password", () => Task.FromResult("token"))
            {
                InnerHandler = recordingHandler
            };

            using var invoker = new HttpMessageInvoker(sut);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/api");

            await invoker.SendAsync(request, CancellationToken.None);
            var firstRequestId = request.Headers.GetValues(RequestIdHeader).Single();

            await invoker.SendAsync(request, CancellationToken.None);
            var secondRequestId = request.Headers.GetValues(RequestIdHeader).Single();

            recordingHandler.CallCount.ShouldBe(2);
            secondRequestId.ShouldNotBe(firstRequestId);
        }
    }
}
