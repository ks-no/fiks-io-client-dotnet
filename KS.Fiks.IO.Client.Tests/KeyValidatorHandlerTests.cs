using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Catalog;
using Microsoft.Extensions.Logging;
using Moq;
using Org.BouncyCastle.X509;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests
{
    public class KeyValidatorHandlerTests
    {
        private readonly X509Certificate _matchingCertificate;
        private readonly string _matchingPrivateKeyPem;
        private readonly string _nonMatchingPrivateKeyPem;

        public KeyValidatorHandlerTests()
        {
            _matchingCertificate = X509CertificateReader.ExtractCertificate(File.ReadAllText("fiks_demo_public.pem"));
            _matchingPrivateKeyPem = File.ReadAllText("fiks_demo_private.pem");
            _nonMatchingPrivateKeyPem = File.ReadAllText("fiks_demo_nomatching_private.pem");
        }

        [Fact]
        public async Task ReturnsTrueWhenPublicKeyMatchesPrivateKey()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var kontoId = Guid.NewGuid();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_matchingCertificate);

            var kontoConfig = new KontoConfiguration(kontoId, _matchingPrivateKeyPem);
            var sut = new KeyValidatorHandler(catalogMock.Object, kontoConfig);

            var result = await sut.ValidatePublicKeyAgainstPrivateKeyAsync();

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ReturnsFalseWhenPrivateKeyDoesNotMatchPublicKey()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var kontoId = Guid.NewGuid();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_matchingCertificate);

            var kontoConfig = new KontoConfiguration(kontoId, _nonMatchingPrivateKeyPem);
            var sut = new KeyValidatorHandler(catalogMock.Object, kontoConfig);

            var result = await sut.ValidatePublicKeyAgainstPrivateKeyAsync();

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task LogsWarningWhenPrivateKeyDoesNotMatchPublicKey()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var kontoId = Guid.NewGuid();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_matchingCertificate);

            var loggerMock = new Mock<ILogger<KeyValidatorHandler>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kontoConfig = new KontoConfiguration(kontoId, _nonMatchingPrivateKeyPem);
            var sut = new KeyValidatorHandler(catalogMock.Object, kontoConfig, loggerFactoryMock.Object);

            await sut.ValidatePublicKeyAgainstPrivateKeyAsync();

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
        public async Task ThrowsWhenNoPrivateKeysAreConfigured()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var kontoId = Guid.NewGuid();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ReturnsAsync(_matchingCertificate);

            var kontoConfig = new KontoConfiguration(kontoId, _matchingPrivateKeyPem);
            kontoConfig.PrivatNokler.Clear();

            var sut = new KeyValidatorHandler(catalogMock.Object, kontoConfig);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ValidatePublicKeyAgainstPrivateKeyAsync());
        }

        [Fact]
        public async Task ThrowsWhenCatalogLookupFails()
        {
            var catalogMock = new Mock<ICatalogHandler>();
            var kontoId = Guid.NewGuid();
            catalogMock.Setup(_ => _.GetPublicKey(kontoId)).ThrowsAsync(new Exception("catalog unavailable"));

            var kontoConfig = new KontoConfiguration(kontoId, _matchingPrivateKeyPem);
            var sut = new KeyValidatorHandler(catalogMock.Object, kontoConfig);

            await Assert.ThrowsAsync<Exception>(() => sut.ValidatePublicKeyAgainstPrivateKeyAsync());
        }
    }
}
