using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Catalog;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Catalog
{
    public class PublicKeySynchronizerTests
    {
        private readonly string _publicKeyPem;
        private readonly X509Certificate _publicKeyCert;
        private readonly string _matchingPrivateKeyPem;
        private readonly string _nonMatchingPrivateKeyPem;

        public PublicKeySynchronizerTests()
        {
            _publicKeyPem = File.ReadAllText("fiks_demo_public.pem");
            _publicKeyCert = X509CertificateReader.ExtractCertificate(_publicKeyPem);
            _matchingPrivateKeyPem = File.ReadAllText("fiks_demo_private.pem");
            _nonMatchingPrivateKeyPem = File.ReadAllText("fiks_demo_nomatching_private.pem");
        }

        [Fact]
        public async Task SkipsUploadWhenOffentligNokkelIsNull()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(Guid.NewGuid(), null);

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            catalogMock.Verify(_ => _.GetPublicKey(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task SkipsUploadWhenOffentligNokkelIsWhitespace()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(Guid.NewGuid(), "   ");

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SkipsUploadWhenCatalogKeyMatchesConfiguredKey()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_publicKeyCert);

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UploadsWhenCatalogReturnsNull()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync((X509Certificate)null);

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(kontoId, _publicKeyPem), Times.Once);
        }

        [Fact]
        public async Task UploadsWhenCatalogThrowsException()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Nøkkel ikke funnet"));

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(kontoId, _publicKeyPem), Times.Once);
        }

        [Fact]
        public async Task SkipsUploadWhenCatalogHasUnrelatedKey()
        {
            var kontoId = Guid.NewGuid();
            var unrelatedCert = GenerateSelfSignedCert();

            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(unrelatedCert);

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UploadsWhenCatalogKeyBelongsToOurKeyring()
        {
            var kontoId = Guid.NewGuid();
            var newCertPem = CertToPem(GenerateSelfSignedCert());

            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_publicKeyCert);

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, newCertPem);

            catalogMock.Verify(_ => _.UploadPublicKey(kontoId, newCertPem), Times.Once);
        }

        [Fact]
        public async Task SkipsUploadWhenConfiguredKeyDoesNotMatchPrivateKey()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Ingen nøkkel"));

            var sut = CreateSut(catalogMock, useNonMatchingPrivateKey: true);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ThrowsAndDoesNotUploadWhenConfiguredPemIsInvalid()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var sut = CreateSut(catalogMock);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.SynchronizePublicKeyAsync(Guid.NewGuid(), "dette er ikke et gyldig PEM-sertifikat"));

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task PropagatesExceptionWhenUploadFails()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Ingen nøkkel"));
            catalogMock.Setup(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Opplasting feilet"));

            var sut = CreateSut(catalogMock);

            await Assert.ThrowsAsync<Exception>(() => sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem));
        }

        [Fact]
        public async Task LogsWarningWhenCatalogLookupFails()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Katalog utilgjengelig"));

            var loggerMock = new Mock<ILogger<PublicKeySynchronizer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var sut = CreateSut(catalogMock, loggerFactory: loggerFactoryMock.Object);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains(kontoId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>() !),
                Times.Once);
        }

        [Fact]
        public async Task LogsInformationWhenUploadSucceeds()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync((X509Certificate)null);

            var loggerMock = new Mock<ILogger<PublicKeySynchronizer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var sut = CreateSut(catalogMock, loggerFactory: loggerFactoryMock.Object);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>() !),
                Times.Exactly(2));
        }

        private PublicKeySynchronizer CreateSut(
            Mock<ICatalogHandler> catalogMock,
            bool useNonMatchingPrivateKey = false,
            ILoggerFactory loggerFactory = null)
        {
            var privateKey = useNonMatchingPrivateKey ? _nonMatchingPrivateKeyPem : _matchingPrivateKeyPem;
            var kontoConfig = new KontoConfiguration(Guid.NewGuid(), privateKey);
            var keyValidator = new KeyValidatorHandler(catalogMock.Object, kontoConfig);
            return new PublicKeySynchronizer(catalogMock.Object, keyValidator, loggerFactory);
        }

        private static string CertToPem(X509Certificate cert)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-----BEGIN CERTIFICATE-----");
            sb.AppendLine(Convert.ToBase64String(cert.GetEncoded(), Base64FormattingOptions.InsertLineBreaks));
            sb.AppendLine("-----END CERTIFICATE-----");
            return sb.ToString();
        }

        private static X509Certificate GenerateSelfSignedCert()
        {
            var random = new SecureRandom();
            var keypairGen = new RsaKeyPairGenerator();
            keypairGen.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(random, 2048));
            var keypair = keypairGen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            var name = new Org.BouncyCastle.Asn1.X509.X509Name("CN=TestCert");
            gen.SetSubjectDN(name);
            gen.SetIssuerDN(name);
            gen.SetNotBefore(DateTime.UtcNow.AddDays(-1));
            gen.SetNotAfter(DateTime.UtcNow.AddDays(365));
            gen.SetPublicKey(keypair.Public);

            var signer = new Asn1SignatureFactory("SHA256WithRSA", keypair.Private, random);
            return gen.Generate(signer);
        }
    }
}
