using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Client.Catalog;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Catalog;
using KS.Fiks.IO.Send.Client.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
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
        public async Task UploadsWhenCatalogHasNoKey()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId))
                .ThrowsAsync(new FiksIOSendPublicKeyNotFoundException("no key"));

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(kontoId, _publicKeyPem), Times.Once);
        }

        [Fact]
        public async Task PropagatesAndDoesNotUploadWhenCatalogFailsTransiently()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Katalog utilgjengelig"));

            var sut = CreateSut(catalogMock);

            await Assert.ThrowsAsync<Exception>(() => sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem));

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SkipsUploadWhenCatalogKeyHasSamePublicKeyButReEncodedCertificate()
        {
            var kontoId = Guid.NewGuid();

            var reEncodedCatalogCert = ReEncodeWithSamePublicKey(_publicKeyCert);

            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(reEncodedCatalogCert);

            var sut = CreateSut(catalogMock);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
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
            var (newCert, newPrivKeyPem) = GenerateKeyPair();
            var newCertPem = CertToPem(newCert);

            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_publicKeyCert);

            // Keyring has old key (matches catalog) AND new key (matches configured cert)
            var sut = CreateSut(catalogMock, additionalPrivateKeys: new[] { newPrivKeyPem });

            await sut.SynchronizePublicKeyAsync(kontoId, newCertPem);

            catalogMock.Verify(_ => _.UploadPublicKey(kontoId, newCertPem), Times.Once);
        }

        [Fact]
        public async Task ThrowsWhenConfiguredKeyDoesNotMatchPrivateKeyDuringRotation()
        {
            var kontoId = Guid.NewGuid();
            var unrelatedConfiguredCertPem = CertToPem(GenerateSelfSignedCert());

            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_publicKeyCert);

            // Keyring matches catalog cert (first check passes), but not the configured cert (second fails)
            var sut = CreateSut(catalogMock);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.SynchronizePublicKeyAsync(kontoId, unrelatedConfiguredCertPem));

            catalogMock.Verify(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ThrowsWhenConfiguredKeyDoesNotMatchPrivateKey()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId))
                .ThrowsAsync(new FiksIOSendPublicKeyNotFoundException("no key"));

            var sut = CreateSut(catalogMock, useNonMatchingPrivateKey: true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem));

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
            catalogMock.Setup(_ => _.GetPublicKey(kontoId))
                .ThrowsAsync(new FiksIOSendPublicKeyNotFoundException("no key"));
            catalogMock.Setup(_ => _.UploadPublicKey(It.IsAny<Guid>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Opplasting feilet"));

            var sut = CreateSut(catalogMock);

            await Assert.ThrowsAsync<Exception>(() => sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem));
        }

        [Fact]
        public async Task DoesNotSwallowTransientCatalogError()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("Katalog utilgjengelig"));

            var sut = CreateSut(catalogMock);

            // A transient error must not be interpreted as "no key" — it propagates so startup can abort.
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem));
            Assert.Equal("Katalog utilgjengelig", ex.Message);
        }

        [Fact]
        public async Task LogsInformationWhenUploadSucceeds()
        {
            var kontoId = Guid.NewGuid();
            var catalogMock = new Mock<ICatalogHandler>();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId))
                .ThrowsAsync(new FiksIOSendPublicKeyNotFoundException("no key"));

            var loggerMock = new Mock<ILogger<PublicKeySynchronizer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var sut = CreateSut(catalogMock, loggerFactory: loggerFactoryMock.Object);

            await sut.SynchronizePublicKeyAsync(kontoId, _publicKeyPem);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Uploading public key")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>() !),
                Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Public key uploaded")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>() !),
                Times.Once);
        }

        private PublicKeySynchronizer CreateSut(
            Mock<ICatalogHandler> catalogMock,
            bool useNonMatchingPrivateKey = false,
            ILoggerFactory loggerFactory = null,
            IEnumerable<string> additionalPrivateKeys = null)
        {
            var primaryKey = useNonMatchingPrivateKey ? _nonMatchingPrivateKeyPem : _matchingPrivateKeyPem;
            var allKeys = additionalPrivateKeys != null
                ? new[] { primaryKey }.Concat(additionalPrivateKeys)
                : new[] { primaryKey };
            var kontoConfig = new KontoConfiguration(Guid.NewGuid(), allKeys);
            var keyValidator = new KeyValidatorHandler(catalogMock.Object, kontoConfig);
            return new PublicKeySynchronizer(catalogMock.Object, keyValidator, loggerFactory);
        }

        private static string CertToPem(X509Certificate cert)
        {
            using var sw = new StringWriter();
            new Org.BouncyCastle.OpenSsl.PemWriter(sw).WriteObject(cert);
            return sw.ToString();
        }

        private static (X509Certificate cert, string privateKeyPem) GenerateKeyPair()
        {
            var random = new SecureRandom();
            var keypairGen = new RsaKeyPairGenerator();
            keypairGen.Init(new KeyGenerationParameters(random, 2048));
            var keypair = keypairGen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            var name = new X509Name("CN=TestCert");
            gen.SetSubjectDN(name);
            gen.SetIssuerDN(name);
            gen.SetNotBefore(DateTime.UtcNow.AddDays(-1));
            gen.SetNotAfter(DateTime.UtcNow.AddDays(365));
            gen.SetPublicKey(keypair.Public);

            var signer = new Asn1SignatureFactory("SHA256WithRSA", keypair.Private, random);
            var cert = gen.Generate(signer);

            using var sw = new StringWriter();
            new PemWriter(sw).WriteObject(keypair.Private);
            return (cert, sw.ToString());
        }

        private static X509Certificate GenerateSelfSignedCert()
        {
            var (cert, _) = GenerateKeyPair();
            return cert;
        }

        private static X509Certificate ReEncodeWithSamePublicKey(X509Certificate source)
        {
            var random = new SecureRandom();
            var signerKeyGen = new RsaKeyPairGenerator();
            signerKeyGen.Init(new KeyGenerationParameters(random, 2048));
            var signerKeys = signerKeyGen.GenerateKeyPair();

            var gen = new X509V3CertificateGenerator();
            gen.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
            var name = new X509Name("CN=ReEncoded");
            gen.SetSubjectDN(name);
            gen.SetIssuerDN(name);
            gen.SetNotBefore(DateTime.UtcNow.AddDays(-5));
            gen.SetNotAfter(DateTime.UtcNow.AddDays(730));
            gen.SetPublicKey(source.GetPublicKey());

            var signer = new Asn1SignatureFactory("SHA256WithRSA", signerKeys.Private, random);
            return gen.Generate(signer);
        }
    }
}
