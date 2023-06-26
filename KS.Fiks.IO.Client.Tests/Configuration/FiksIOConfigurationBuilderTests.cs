using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using KS.Fiks.IO.Client.Configuration;
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

            configuration.ApiConfiguration.Host.Should().Be(ApiConfiguration.TestHost);
            configuration.AmqpConfiguration.Host.Should().Be(AmqpConfiguration.TestHost);
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

            configuration.ApiConfiguration.Host.Should().Be(ApiConfiguration.ProdHost);
            configuration.AmqpConfiguration.Host.Should().Be(AmqpConfiguration.ProdHost);
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

            config.KontoConfiguration.PrivatNokler.Single().Should().Be(dummyPrivateKey);
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

            config.KontoConfiguration.PrivatNokler.Should().BeEquivalentTo(dummyPrivateKeys);
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