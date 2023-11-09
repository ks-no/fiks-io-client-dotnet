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
        public void PasswordIsSetToIntegrationPasswordAndMaskinportenToken()
        {
            var password = "myIntegrationPassword";
            var token = "maskinportenExpectedToken";
            var sut = _fixture.WithMaskinportenToken(token).WithIntegrationPassword(password).CreateSut();
            Assert.Equal($"{password} {token}", sut.Password);
        }
    }
}