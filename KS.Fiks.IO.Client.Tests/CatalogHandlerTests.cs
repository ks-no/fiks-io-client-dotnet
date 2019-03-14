using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using Moq;
using Moq.Protected;
using Xunit;

namespace KS.Fiks.IO.Client.Tests
{
    public class CatalogHandlerTests
    {
        private readonly CatalogHandlerFixture _fixture;

        public CatalogHandlerTests()
        {
            _fixture = new CatalogHandlerFixture();
        }

        [Fact]
        public async Task GetsExpectedAccount()
        {
            var expectedAccount = new AccountResponse
            {
                AccountId = Guid.NewGuid(),
                AccountName = "accountName",
                OrgId = Guid.NewGuid(),
                OrgName = "orgName",
                Status = new AccountResponseStatus
                {
                    Message = "No message",
                    ValidSender = true,
                    ValidReceiver = false
                }
            };
            var sut = _fixture.WithAccountResponse(expectedAccount).CreateSut();

            var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

            result.OrgId.Should().Be(expectedAccount.OrgId);
            result.OrgName.Should().Be(expectedAccount.OrgName);
            result.AccountId.Should().Be(expectedAccount.AccountId);
            result.AccountName.Should().Be(expectedAccount.AccountName);
            result.IsValidSender.Should().Be(expectedAccount.Status.ValidSender);
            result.IsValidReceiver.Should().Be(expectedAccount.Status.ValidReceiver);
        }

        [Fact]
        public async Task CallsExpectedUri()
        {
            var host = "api.fiks.dev.ks.no";
            var port = 443;
            var scheme = "https";
            var path = "/svarinn2/katalog/api/v1";

            var sut = _fixture.WithHost(host).WithPort(port).WithScheme(scheme).WithPath(path).CreateSut();

            var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.Port == port &&
                    req.RequestUri.Host == host &&
                    req.RequestUri.Scheme == scheme &&
                    req.RequestUri.AbsolutePath == path + "/lookup"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SetsExpectedAuthorizationHeader()
        {
            var expectedToken = Guid.NewGuid().ToString();

            var sut = _fixture.WithAccessToken(expectedToken).CreateSut();

            var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(
                    req =>
                        req.Headers.Authorization.Parameter == expectedToken &&
                        req.Headers.Authorization.Scheme == "Bearer"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SetsExpectedIntegrasjonIdAndPassword()
        {
            var expectedId = Guid.NewGuid();
            var expectedPassword = "myIntegrasjonPassword";

            var sut = _fixture.WithIntegrasjonId(expectedId).WithIntegrasjonPassword(expectedPassword).CreateSut();

            var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(
                    req =>
                        req.Headers.GetValues("integrasjonPassord").FirstOrDefault() ==
                        expectedPassword &&
                        req.Headers.GetValues("integrasjonId").FirstOrDefault() ==
                        expectedId.ToString()),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UsesExpectedQueryParams()
        {
            var request = new LookupRequest
            {
                Identifier = "testIdentifier",
                MessageType = "testMessageType",
                AccessLevel = 4
            };

            var sut = _fixture.CreateSut();

            var result = await sut.Lookup(request).ConfigureAwait(false);

            Func<HttpRequestMessage, string, string> queryFromReq = (req, field) => HttpUtility.ParseQueryString(req.RequestUri.Query)[field];
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(
                    (req) =>
                        queryFromReq(req, "identifikator") == request.Identifier &&
                        queryFromReq(req, "meldingType") == request.MessageType &&
                        int.Parse(queryFromReq(req, "sikkerhetsniva")) == request.AccessLevel),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}