using System;
using System.Threading.Tasks;
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
        public async Task GetCredentialsAsync_ShouldHandleExceptionsGracefully()
        {
            var sut = _fixture.WithTokenRetrievalException().CreateSut();

            await Assert.ThrowsAsync<Exception>(() => sut.GetCredentialsAsync());
        }

        [Fact]
        public async Task ExpiredToken_ShouldRequestNewTokenAsync()
        {
            var expiredToken = "expiredToken";
            var newToken = "newValidToken";

            var sut = _fixture
                .WithMaskinportenToken(expiredToken, -10)
                .WithNewToken(newToken)
                .CreateSut();
            await sut.GetCredentialsAsync();

            var credentials = await sut.GetCredentialsAsync();

            Assert.Equal($"{_fixture.IntegrationPassword} {newToken}", credentials.Password);
        }

        [Fact]
        public async Task GetCredentialsAsync_ReturnsCorrectCredentials()
        {
            var password = "myIntegrationPassword";
            var token = "maskinportenExpectedToken";
            var sut = _fixture.WithMaskinportenToken(token).WithIntegrationPassword(password).CreateSut();

            var credentials = await sut.GetCredentialsAsync();

            Assert.Equal($"{password} {token}", credentials.Password);
            Assert.Equal(_fixture.IntegrationId.ToString(), credentials.UserName);
        }
    }
}