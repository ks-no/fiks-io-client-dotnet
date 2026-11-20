using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Configuration
{
    public class FiksIOConfigurationTests
    {
        [Fact]
        public void WorksWithNullApiConfigurationConstructor()
        {
            var result = new FiksIOConfiguration(
                new KontoConfiguration(Guid.NewGuid(),"string"),
                new IntegrasjonConfiguration(Guid.NewGuid(),"password"),
                new MaskinportenClientConfiguration("aud", "tokenedm", "issuer",100, Mock.Of<X509Certificate2>()),
                null);
        }

        [Fact]
        public void WorksWithNullMaskinportenClientConfigurationConstructor()
        {
            var result = new FiksIOConfiguration(
                new KontoConfiguration(Guid.NewGuid(),"string"),
                new IntegrasjonConfiguration(Guid.NewGuid(),"password"),
                null);
        }

        [Fact]
        public void UsesProdAmqpHostWhenNoApiOrAmqpConfigurationIsGiven()
        {
            var result = CreateConfiguration(apiConfiguration: null, amqpConfiguration: null);

            result.ApiConfiguration.Host.ShouldBe("api.fiks.ks.no");
            result.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.ProdHost);
        }

        [Fact]
        public void UsesProdAmqpHostWithProdApiConfiguration()
        {
            var result = CreateConfiguration(ApiConfiguration.CreateProdConfiguration(), amqpConfiguration: null);

            result.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.ProdHost);
        }

        [Fact]
        public void UsesTestAmqpHostWithTestApiConfiguration()
        {
            var result = CreateConfiguration(ApiConfiguration.CreateTestConfiguration(), amqpConfiguration: null);

            result.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.TestHost);
        }

        [Fact]
        public void UsesTestAmqpHostWhenTestApiHostDiffersInCase()
        {
            var result = CreateConfiguration(new ApiConfiguration("https", "API.FIKS.TEST.KS.NO", 443), amqpConfiguration: null);

            result.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.TestHost);
        }

        [Fact]
        public void KeepsApiHostAsAmqpHostForUnknownApiHost()
        {
            var result = CreateConfiguration(ApiConfiguration.CreateDevConfiguration("api.dev.example"), amqpConfiguration: null);

            result.AmqpConfiguration.Host.ShouldBe("api.dev.example");
        }

        [Fact]
        public void UsesGivenAmqpConfiguration()
        {
            var amqpConfiguration = new AmqpConfiguration("amqp.example.local");

            var result = CreateConfiguration(ApiConfiguration.CreateTestConfiguration(), amqpConfiguration);

            result.AmqpConfiguration.ShouldBeSameAs(amqpConfiguration);
        }

        [Fact]
        public void UsesTestAmqpHostWithTestApiConfigurationInConstructorWithoutMaskinporten()
        {
            var result = new FiksIOConfiguration(
                new KontoConfiguration(Guid.NewGuid(), "string"),
                new IntegrasjonConfiguration(Guid.NewGuid(), "password"),
                null,
                apiConfiguration: ApiConfiguration.CreateTestConfiguration());

            result.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.TestHost);
        }

        private static FiksIOConfiguration CreateConfiguration(ApiConfiguration apiConfiguration, AmqpConfiguration amqpConfiguration)
        {
            return new FiksIOConfiguration(
                new KontoConfiguration(Guid.NewGuid(), "string"),
                new IntegrasjonConfiguration(Guid.NewGuid(), "password"),
                new MaskinportenClientConfiguration("aud", "tokenedm", "issuer", 100, Mock.Of<X509Certificate2>()),
                null,
                apiConfiguration: apiConfiguration,
                amqpConfiguration: amqpConfiguration);
        }
    }
}
