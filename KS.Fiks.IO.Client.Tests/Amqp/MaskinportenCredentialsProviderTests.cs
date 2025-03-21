using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Amqp
{
    public class MaskinportenCredentialsProviderTests
    {
        private readonly MaskinportenCredentialsProviderFixture _fixture;

        public MaskinportenCredentialsProviderTests()
        {
            _fixture = new MaskinportenCredentialsProviderFixture();
        }

        [Fact]
        public async Task PasswordIsSetToIntegrationPasswordAndMaskinportenTokenAsync()
        {
            var password = "myIntegrationPassword";
            var token = "maskinportenExpectedToken";

            var sut = _fixture
                .WithMaskinportenToken(token)
                .WithIntegrationPassword(password)
                .CreateSut();

            var credentials = await sut.GetCredentialsAsync();

            Assert.Equal($"{password} {token}", credentials.Password);
        }

        [Fact]
        public async Task GetCredentialsAsyncShouldHandleExceptions()
        {
            var sut = _fixture.WithTokenRetrievalException().CreateSut();

            await Assert.ThrowsAsync<Exception>(() => sut.GetCredentialsAsync());
        }

        [Fact]
        public async Task GetCredentialsAsyncReturnsCorrectCredentials()
        {
            var password = "myIntegrationPassword";
            var token = "maskinportenExpectedToken";
            var sut = _fixture.WithMaskinportenToken(token).WithIntegrationPassword(password).CreateSut();

            var credentials = await sut.GetCredentialsAsync();

            Assert.Equal($"{password} {token}", credentials.Password);
            Assert.Equal(_fixture.IntegrationId.ToString(), credentials.UserName);
        }

        [Fact]
        public async Task ShouldRetrieveNewTokenWhenExpired()
        {
            var expiredToken = "expiredToken";
            var newToken = "newValidToken";

            var sut = _fixture
                .WithMaskinportenToken(expiredToken, -10)
                .WithNewMaskinportenToken(newToken)
                .CreateSut();

            var credentials = await sut.GetCredentialsAsync();
            Assert.Equal($"{_fixture.IntegrationPassword} {expiredToken}", credentials.Password);

            credentials = await sut.GetCredentialsAsync();
            Assert.Equal($"{_fixture.IntegrationPassword} {newToken}", credentials.Password);

            _fixture.MaskinportenClientMock.Verify(client => client.GetAccessToken(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldRetrieveNewTokenWhenRefreshed()
        {
            var expiredToken = "expiredToken";
            var newToken = "newValidToken";

            var sut = _fixture
                .WithMaskinportenToken(expiredToken)
                .WithNewMaskinportenToken(newToken)
                .CreateSut();

            var credentials = await sut.GetCredentialsAsync();
            Assert.Equal($"{_fixture.IntegrationPassword} {expiredToken}", credentials.Password);

            await sut.RefreshAsync();
            credentials = await sut.GetCredentialsAsync();
            Assert.Equal($"{_fixture.IntegrationPassword} {newToken}", credentials.Password);
        }
    }
}