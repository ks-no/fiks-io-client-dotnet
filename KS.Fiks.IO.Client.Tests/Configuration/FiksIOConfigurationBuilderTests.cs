using System;
using System.Linq;
using System.Security.Cryptography;
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
            var exception = Assert.Throws<ArgumentException>(() =>
                FiksIOConfigurationBuilder
                    .Init()
                    .WithAmqpConfiguration("test_app", 10)
                    .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                    .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key")
                    .BuildProdConfiguration());

            exception.Message.ShouldContain("WithMaskinportenConfiguration");
            exception.Message.ShouldContain("WithMaskinportenKeyPairConfiguration");
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

        [Fact]
        public void ProdConfigurationWithMaskinportenKeyPairShouldUseKeyPair()
        {
            using var privateKey = RSA.Create(2048);
            var keyIdentifier = $"{Guid.NewGuid():N}";

            var configuration = CreateBuilderWithoutMaskinportenConfiguration()
                .WithMaskinportenKeyPairConfiguration(privateKey, "test-issuer", keyIdentifier)
                .BuildProdConfiguration();

            var maskinportenConfiguration = configuration.MaskinportenConfiguration;
            maskinportenConfiguration.PrivateKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.PublicKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
            maskinportenConfiguration.Issuer.ShouldBe("test-issuer");
            maskinportenConfiguration.Certificate.ShouldBeNull();
            maskinportenConfiguration.Audience.ShouldBe(FiksIOConfiguration.maskinportenProdAudience);
            maskinportenConfiguration.TokenEndpoint.ShouldBe(FiksIOConfiguration.maskinportenProdTokenEndpoint);
        }

        [Fact]
        public void TestConfigurationWithMaskinportenKeyPairShouldUseKeyPair()
        {
            using var privateKey = RSA.Create(2048);
            var keyIdentifier = $"{Guid.NewGuid():N}";

            var configuration = CreateBuilderWithoutMaskinportenConfiguration()
                .WithMaskinportenKeyPairConfiguration(privateKey, "test-issuer", keyIdentifier)
                .BuildTestConfiguration();

            var maskinportenConfiguration = configuration.MaskinportenConfiguration;
            maskinportenConfiguration.PrivateKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.PublicKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
            maskinportenConfiguration.Issuer.ShouldBe("test-issuer");
            maskinportenConfiguration.Certificate.ShouldBeNull();
            maskinportenConfiguration.Audience.ShouldBe(FiksIOConfiguration.maskinportenTestAudience);
            maskinportenConfiguration.TokenEndpoint.ShouldBe(FiksIOConfiguration.maskinportenTestTokenEndpoint);
        }

        [Fact]
        public void DevConfigurationWithMaskinportenKeyPairShouldUseKeyPair()
        {
            using var privateKey = RSA.Create(2048);
            var keyIdentifier = $"{Guid.NewGuid():N}";

            var configuration = CreateBuilderWithoutMaskinportenConfiguration()
                .WithMaskinportenKeyPairConfiguration(privateKey, "test-issuer", keyIdentifier)
                .BuildDevConfiguration("amqp-host", "api-host");

            var maskinportenConfiguration = configuration.MaskinportenConfiguration;
            maskinportenConfiguration.PrivateKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.PublicKey.ShouldBeSameAs(privateKey);
            maskinportenConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
            maskinportenConfiguration.Issuer.ShouldBe("test-issuer");
            maskinportenConfiguration.Certificate.ShouldBeNull();
            maskinportenConfiguration.Audience.ShouldBe(FiksIOConfiguration.maskinportenTestAudience);
            maskinportenConfiguration.TokenEndpoint.ShouldBe(FiksIOConfiguration.maskinportenTestTokenEndpoint);
        }

        [Fact]
        public void ConfigurationFailsWithBothCertificateAndKeyPairInMaskinportenConfiguration()
        {
            using var privateKey = RSA.Create(2048);

            var exception = Assert.Throws<ArgumentException>(() =>
                CreateBuilderWithoutMaskinportenConfiguration()
                    .WithMaskinportenConfiguration(new X509Certificate2(), "test-issuer")
                    .WithMaskinportenKeyPairConfiguration(privateKey, "test-issuer", "test-key-identifier")
                    .BuildProdConfiguration());

            exception.Message.ShouldContain("WithMaskinportenConfiguration");
            exception.Message.ShouldContain("WithMaskinportenKeyPairConfiguration");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ConfigurationFailsWithoutKeyIdentifierInMaskinportenKeyPairConfiguration(string? keyIdentifier)
        {
            using var privateKey = RSA.Create(2048);

            var exception = Assert.Throws<ArgumentException>(() =>
                CreateBuilderWithoutMaskinportenConfiguration()
                    .WithMaskinportenKeyPairConfiguration(privateKey, "test-issuer", keyIdentifier)
                    .BuildProdConfiguration());

            exception.Message.ShouldContain("keyIdentifier");
        }

        [Fact]
        public void ConfigurationFailsWithoutPrivateKeyInMaskinportenKeyPairConfiguration()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                CreateBuilderWithoutMaskinportenConfiguration()
                    .WithMaskinportenKeyPairConfiguration(null, "test-issuer", "test-key-identifier")
                    .BuildProdConfiguration());

            exception.Message.ShouldContain("WithMaskinportenKeyPairConfiguration");
        }

        [Fact]
        public void ConfigurationFailsWithoutIssuerInMaskinportenKeyPairConfiguration()
        {
            using var privateKey = RSA.Create(2048);

            Assert.Throws<ArgumentException>(() =>
                CreateBuilderWithoutMaskinportenConfiguration()
                    .WithMaskinportenKeyPairConfiguration(privateKey, null, "test-key-identifier")
                    .BuildProdConfiguration());
        }

        private static FiksIOConfigurationBuilder CreateBuilderWithoutMaskinportenConfiguration()
        {
            return FiksIOConfigurationBuilder
                .Init()
                .WithAmqpConfiguration("test_app", 10)
                .WithAsiceSigningConfiguration(new X509Certificate2())
                .WithFiksIntegrasjonConfiguration(Guid.NewGuid(), "passord")
                .WithFiksKontoConfiguration(Guid.NewGuid(), "liksom-en-private-key");
        }
    }
}
