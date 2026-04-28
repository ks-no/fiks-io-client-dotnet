using System;
using System.Linq;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Send.Client.Catalog;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Catalog
{
    internal class PublicKeySynchronizer
    {
        private readonly ICatalogHandler _catalogHandler;
        private readonly IKeyValidator _keyValidatorHandler;
        private readonly ILogger<PublicKeySynchronizer> _logger;

        public PublicKeySynchronizer(
            ICatalogHandler catalogHandler,
            IKeyValidator keyValidatorHandler,
            ILoggerFactory loggerFactory = null)
        {
            _catalogHandler = catalogHandler;
            _keyValidatorHandler = keyValidatorHandler;
            _logger = loggerFactory?.CreateLogger<PublicKeySynchronizer>();
        }

        public async Task SynchronizePublicKeyAsync(Guid kontoId, string configuredPublicKeyPem)
        {
            if (string.IsNullOrWhiteSpace(configuredPublicKeyPem))
            {
                _logger?.LogDebug("No public key configured for account {KontoId}, skipping upload.", kontoId);
                return;
            }

            X509Certificate configuredCert;
            try
            {
                configuredCert = X509CertificateReader.ExtractCertificate(configuredPublicKeyPem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Public key configured for account {kontoId} is not a valid X.509 certificate.", ex);
            }

            X509Certificate catalogCert = null;
            bool needsUpload;
            try
            {
                catalogCert = await _catalogHandler.GetPublicKey(kontoId).ConfigureAwait(false);
                needsUpload = catalogCert == null ||
                              !catalogCert.GetEncoded().SequenceEqual(configuredCert.GetEncoded());
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex,
                    "Failed to retrieve public key from catalog for account {KontoId}. Attempting upload.",
                    kontoId);
                needsUpload = true;
            }

            if (!needsUpload)
            {
                _logger?.LogDebug("Public key for account {KontoId} is already up to date, skipping upload.", kontoId);
                return;
            }

            if (catalogCert != null && !_keyValidatorHandler.ValidateCertificateAgainstPrivateKeys(catalogCert))
            {
                _logger?.LogWarning("Catalog public key for account {KontoId} does not belong to this client's key ring. Skipping upload.", kontoId);
                return;
            }

            if (!_keyValidatorHandler.ValidateCertificateAgainstPrivateKeys(configuredCert))
            {
                _logger?.LogWarning("Configured public key for account {KontoId} does not match any configured private key. Skipping upload.", kontoId);
                return;
            }

            _logger?.LogInformation("Uploading public key for account {KontoId}.", kontoId);
            await _catalogHandler.UploadPublicKey(kontoId, configuredPublicKeyPem).ConfigureAwait(false);
            _logger?.LogInformation("Public key uploaded for account {KontoId}.", kontoId);
        }
    }
}
