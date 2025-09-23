using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Configuration
{
    public class FiksIOConfigurationBuilderTests
    {
        [Fact]
        public void TestConfigurationWithAllRequiredConfigurations()
        {
            var configuration = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("test_app", 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                .BuildTestConfiguration();

            configuration.ApiConfiguration.Host.ShouldBe(ApiConfiguration.TestHost);
            configuration.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.TestHost);
        }

        [Fact]
        public void TestConfigurationWithMaskinportenKeyIdentifierProvidedShouldBeCreatedWithKeyIdentifier()
        {
            var keyIdentifier = $"{Guid.NewGuid():N}";
            var configuration = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("test_app", 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer", keyIdentifier)
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                .BuildTestConfiguration();

            configuration.MaskinportenConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
        }

        [Fact]
        public void ProdConfigurationWithAllRequiredConfigurations()
        {
            var configuration = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("test_app", 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                .BuildProdConfiguration();

            configuration.ApiConfiguration.Host.ShouldBe(ApiConfiguration.ProdHost);
            configuration.AmqpConfiguration.Host.ShouldBe(AmqpConfiguration.ProdHost);
        }

        [Fact]
        public void ProdConfigurationWithWithMaskinportenKeyIdentifierProvidedShouldBeCreatedWithKeyIdentifier()
        {
            var keyIdentifier = $"{Guid.NewGuid():N}";
            var configuration = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("test_app", 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer", keyIdentifier)
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                .BuildProdConfiguration();

            configuration.MaskinportenConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
        }

        [Fact]
        public void ConfigWithSinglPrivateKey()
        {
            var dummyPrivateKey = Guid.NewGuid().ToString();
            var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration(Guid.NewGuid().ToString(), 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), Guid.NewGuid().ToString())
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), Guid.NewGuid().ToString())
                .WithFiksKontoConfiguration(Guid.NewGuid(), dummyPrivateKey)
                .BuildTestConfiguration();

            config.KontoConfiguration.PrivatNokler.Single().ShouldBe(dummyPrivateKey);
        }

        [Fact]
        public void ConfigWithMultiplePrivateKeys()
        {
            var dummyPrivateKeys = Enumerable.Range(0, 3).Select(_ => Guid.NewGuid().ToString()).ToList();
            var config = FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration(Guid.NewGuid().ToString(), 10)
                .WithMaskinportenConfiguration(new X509Certificate2(), Guid.NewGuid().ToString())
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), Guid.NewGuid().ToString())
                .WithFiksKontoConfiguration(Guid.NewGuid(), dummyPrivateKeys)
                .BuildTestConfiguration();

            config.KontoConfiguration.PrivatNokler.ShouldBeEquivalentTo(dummyPrivateKeys);
        }

        [Fact]
        public void ConfigurationFailsWithoutCertificateInMaskinportenConfiguration()
        {
            Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithMaskinportenConfiguration(null, "test-issuer")
                    .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                    .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                    .BuildProdConfiguration());
        }

        [Fact]
        public void ConfigurationFailsWithoutAsiceSigningConfiguration()
        {
            Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                    .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                    .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                    .BuildProdConfiguration());
        }

        [Fact]
        public void ConfigurationFailsWithoutMaskinportenConfiguration()
        {
            Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                    .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                    .BuildProdConfiguration());
        }

        [Fact]
        public void ConfigurationFailsWithoutFiksIntegrasjonConfiguration()
        {
            Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                    .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                    .BuildProdConfiguration());
        }

        [Fact]
        public void ConfigurationFailsWithoutFiksKontoConfiguration()
        {
            Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                    .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                    .BuildProdConfiguration());
        }
    }
}