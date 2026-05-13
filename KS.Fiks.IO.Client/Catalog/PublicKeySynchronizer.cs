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

            var configuredCert = ParseConfiguredCertificate(kontoId, configuredPublicKeyPem);
            var catalogCert = await FetchCatalogCertificateAsync(kontoId).ConfigureAwait(false);

            if (IsAlreadyUpToDate(kontoId, catalogCert, configuredCert))
            {
                return;
            }

            if (catalogCert != null && !CatalogKeyBelongsToUs(kontoId, catalogCert))
            {
                return;
            }

            EnsureConfiguredCertIsOurs(kontoId, configuredCert);

            _logger?.LogInformation("Uploading public key for account {KontoId}.", kontoId);
            await _catalogHandler.UploadPublicKey(kontoId, configuredPublicKeyPem).ConfigureAwait(false);
            _logger?.LogInformation("Public key uploaded for account {KontoId}.", kontoId);
        }

        private X509Certificate ParseConfiguredCertificate(Guid kontoId, string pem)
        {
            try
            {
                return X509CertificateReader.ExtractCertificate(pem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Public key configured for account {kontoId} is not a valid X.509 certificate.", ex);
            }
        }

        private async Task<X509Certificate> FetchCatalogCertificateAsync(Guid kontoId)
        {
            try
            {
                return await _catalogHandler.GetPublicKey(kontoId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "Failed to retrieve public key from catalog for account {KontoId}. Attempting upload.",
                    kontoId);
                return null;
            }
        }

        private bool IsAlreadyUpToDate(Guid kontoId, X509Certificate catalogCert, X509Certificate configuredCert)
        {
            if (catalogCert == null || !catalogCert.GetEncoded().SequenceEqual(configuredCert.GetEncoded()))
            {
                return false;
            }

            _logger?.LogDebug("Public key for account {KontoId} is already up to date, skipping upload.", kontoId);
            return true;
        }

        private bool CatalogKeyBelongsToUs(Guid kontoId, X509Certificate catalogCert)
        {
            if (_keyValidatorHandler.ValidateCertificateAgainstPrivateKeys(catalogCert))
            {
                return true;
            }

            _logger?.LogWarning(
                "Catalog public key for account {KontoId} does not belong to this client's key ring. Skipping upload.",
                kontoId);
            return false;
        }

        private void EnsureConfiguredCertIsOurs(Guid kontoId, X509Certificate configuredCert)
        {
            if (!_keyValidatorHandler.ValidateCertificateAgainstPrivateKeys(configuredCert))
            {
                throw new InvalidOperationException(
                    $"Configured public key for account {kontoId} does not match any configured private key.");
            }
        }
    }
}
