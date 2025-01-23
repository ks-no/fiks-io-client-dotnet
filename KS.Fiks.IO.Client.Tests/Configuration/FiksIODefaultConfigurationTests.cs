using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Configuration
{
    public class FiksIODefaultConfigurationTests
    {
        [Fact]
        public void DefaultConfigurationForProd()
        {

            var integrationId = Guid.NewGuid();
            var integrationPassord = Guid.NewGuid().ToString();
            var kontoId = Guid.NewGuid();
            var privatNokkel = Guid.NewGuid().ToString();
            var issuer = Guid.NewGuid().ToString();
            var maskinportenSertifikat = new X509Certificate2();
            var asiceSertifikat = new X509Certificate2();

            var config = FiksIOConfiguration.CreateProdConfiguration(
                integrasjonId: integrationId,
                integrasjonPassord: integrationPassord,
                kontoId: kontoId,
                privatNokkel: privatNokkel,
                issuer: issuer,
                maskinportenSertifikat: maskinportenSertifikat,
                asiceSertifikat: asiceSertifikat);

            config.IntegrasjonConfiguration.IntegrasjonId.ShouldBe(integrationId);
            config.IntegrasjonConfiguration.IntegrasjonPassord.ShouldBe(integrationPassord);
            config.AmqpConfiguration.Host.ShouldBe("io.fiks.ks.no");
            config.ApiConfiguration.Host.ShouldBe("api.fiks.ks.no");
        }

        [Fact]
        public void DefaultConfigurationWithKeepAliveForProd()
        {

            var integrationId = Guid.NewGuid();
            var integrationPassord = Guid.NewGuid().ToString();
            var kontoId = Guid.NewGuid();
            var privatNokkel = Guid.NewGuid().ToString();
            var issuer = Guid.NewGuid().ToString();
            var maskinportenSertifikat = new X509Certificate2();
            var asiceSertifikat = new X509Certificate2();

            var config = FiksIOConfiguration.CreateProdConfiguration(
                integrasjonId: integrationId,
                integrasjonPassord: integrationPassord,
                kontoId: kontoId,
                privatNokkel: privatNokkel,
                issuer: issuer,
                maskinportenSertifikat: maskinportenSertifikat,
                asiceSertifikat: asiceSertifikat);

            config.IntegrasjonConfiguration.IntegrasjonId.ShouldBe(integrationId);
            config.IntegrasjonConfiguration.IntegrasjonPassord.ShouldBe(integrationPassord);
            config.AmqpConfiguration.Host.ShouldBe("io.fiks.ks.no");
            config.ApiConfiguration.Host.ShouldBe("api.fiks.ks.no");
        }

        [Fact]
        public void DefaultConfigurationForTest()
        {

            var integrationId = Guid.NewGuid();
            var integrationPassord = Guid.NewGuid().ToString();
            var kontoId = Guid.NewGuid();
            var privatNokkel = Guid.NewGuid().ToString();
            var issuer = Guid.NewGuid().ToString();
            var maskinportenSertifikat = new X509Certificate2();
            var asiceSertifikat = new X509Certificate2();

            var config = FiksIOConfiguration.CreateTestConfiguration(
                fiksIntegrasjonId: integrationId,
                fiksIntegrasjonPassord: integrationPassord,
                fiksKontoId: kontoId,
                privatNokkel: privatNokkel,
                issuer: issuer,
                maskinportenSertifikat: maskinportenSertifikat,
                asiceSertifikat: asiceSertifikat
            );

            config.IntegrasjonConfiguration.IntegrasjonId.ShouldBe(integrationId);
            config.IntegrasjonConfiguration.IntegrasjonPassord.ShouldBe(integrationPassord);
            config.AmqpConfiguration.Host.ShouldBe("io.fiks.test.ks.no");
            config.ApiConfiguration.Host.ShouldBe("api.fiks.test.ks.no");
        }

        [Fact]
        public void DefaultConfigurationWithKeepAliveForTest()
        {

            var integrationId = Guid.NewGuid();
            var integrationPassord = Guid.NewGuid().ToString();
            var kontoId = Guid.NewGuid();
            var privatNokkel = Guid.NewGuid().ToString();
            var issuer = Guid.NewGuid().ToString();
            var maskinportenSertifikat = new X509Certificate2();
            var asiceSertifikat = new X509Certificate2();

            var config = FiksIOConfiguration.CreateTestConfiguration(
                fiksIntegrasjonId: integrationId,
                fiksIntegrasjonPassord: integrationPassord,
                fiksKontoId: kontoId,
                privatNokkel: privatNokkel,
                issuer: issuer,
                maskinportenSertifikat: maskinportenSertifikat,
                asiceSertifikat: asiceSertifikat
            );

            config.IntegrasjonConfiguration.IntegrasjonId.ShouldBe(integrationId);
            config.IntegrasjonConfiguration.IntegrasjonPassord.ShouldBe(integrationPassord);
            config.AmqpConfiguration.Host.ShouldBe("io.fiks.test.ks.no");
            config.ApiConfiguration.Host.ShouldBe("api.fiks.test.ks.no");
        }
    }
}