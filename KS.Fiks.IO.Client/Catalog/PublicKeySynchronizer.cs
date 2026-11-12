using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Send.Client.Catalog;
using KS.Fiks.IO.Send.Client.Exceptions;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Catalog
{
    internal class PublicKeySynchronizer
    {
        // The catalog may need a moment to see a newly uploaded key, for example when the upload
        // activates a deactivated account. Confirmation is retried after these delays before warning.
        internal static readonly IReadOnlyList<TimeSpan> DefaultConfirmationRetryDelays = new[]
        {
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
        };

        private readonly ICatalogHandler _catalogHandler;
        private readonly IKeyValidator _keyValidator;
        private readonly ILogger<PublicKeySynchronizer> _logger;
        private readonly IReadOnlyList<TimeSpan> _confirmationRetryDelays;

        public PublicKeySynchronizer(
            ICatalogHandler catalogHandler,
            IKeyValidator keyValidator,
            ILoggerFactory loggerFactory = null,
            IReadOnlyList<TimeSpan> confirmationRetryDelays = null)
        {
            _catalogHandler = catalogHandler;
            _keyValidator = keyValidator;
            _logger = loggerFactory?.CreateLogger<PublicKeySynchronizer>();
            _confirmationRetryDelays = confirmationRetryDelays ?? DefaultConfirmationRetryDelays;
        }

        public async Task<X509Certificate> SynchronizePublicKeyAsync(Guid kontoId, string configuredPublicKeyPem)
        {
            if (string.IsNullOrWhiteSpace(configuredPublicKeyPem))
            {
                _logger?.LogDebug("No public key configured for account {KontoId}, skipping upload.", kontoId);
                return null;
            }

            var configuredCert = ParseConfiguredCertificate(kontoId, configuredPublicKeyPem);
            var catalogCert = await FetchCatalogCertificateAsync(kontoId).ConfigureAwait(false);

            if (IsAlreadyUpToDate(kontoId, catalogCert, configuredCert))
            {
                return catalogCert;
            }

            if (catalogCert != null && !CatalogKeyBelongsToUs(kontoId, catalogCert))
            {
                return catalogCert;
            }

            EnsureConfiguredCertIsOurs(kontoId, configuredCert);

            _logger?.LogInformation("Uploading public key for account {KontoId}.", kontoId);
            try
            {
                await _catalogHandler.UploadPublicKey(kontoId, configuredPublicKeyPem).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to upload public key for account {KontoId}.", kontoId);
                throw;
            }

            _logger?.LogInformation("Public key uploaded for account {KontoId}.", kontoId);

            await ConfirmUploadAsync(kontoId, configuredCert).ConfigureAwait(false);

            return configuredCert;
        }

        private async Task ConfirmUploadAsync(Guid kontoId, X509Certificate configuredCert)
        {
            var maxAttempts = _confirmationRetryDelays.Count + 1;
            Exception lastException = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt > 1)
                {
                    await Task.Delay(_confirmationRetryDelays[attempt - 2]).ConfigureAwait(false);
                }

                try
                {
                    var storedCert = await _catalogHandler.GetPublicKey(kontoId).ConfigureAwait(false);
                    if (HasSamePublicKey(storedCert, configuredCert))
                    {
                        _logger?.LogDebug(
                            "Catalog confirmed public key for account {KontoId} after {Attempts} attempt(s).",
                            kontoId,
                            attempt);
                        return;
                    }

                    lastException = null;
                    _logger?.LogDebug(
                        "Catalog public key for account {KontoId} does not match the uploaded key yet (attempt {Attempt} of {MaxAttempts}).",
                        kontoId,
                        attempt,
                        maxAttempts);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger?.LogDebug(
                        "Could not confirm catalog public key for account {KontoId} yet (attempt {Attempt} of {MaxAttempts}): {Reason}",
                        kontoId,
                        attempt,
                        maxAttempts,
                        ex.Message);
                }
            }

            if (lastException != null)
            {
                _logger?.LogWarning(
                    lastException,
                    "Could not confirm catalog public key for account {KontoId} after upload ({Attempts} attempts).",
                    kontoId,
                    maxAttempts);
            }
            else
            {
                _logger?.LogWarning(
                    "Catalog public key for account {KontoId} does not match the key just uploaded after {Attempts} attempts (may be eventual consistency).",
                    kontoId,
                    maxAttempts);
            }
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
            catch (FiksIOSendPublicKeyNotFoundException)
            {
                _logger?.LogDebug(
                    "No public key registered in catalog for account {KontoId}. The configured key will be uploaded.",
                    kontoId);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Failed to read public key from catalog for account {KontoId}; cannot safely synchronize. Aborting startup.",
                    kontoId);
                throw;
            }
        }

        private bool IsAlreadyUpToDate(Guid kontoId, X509Certificate catalogCert, X509Certificate configuredCert)
        {
            if (!HasSamePublicKey(catalogCert, configuredCert))
            {
                return false;
            }

            _logger?.LogDebug("Public key for account {KontoId} is already up to date, skipping upload.", kontoId);
            return true;
        }

        private static bool HasSamePublicKey(X509Certificate first, X509Certificate second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            return first.CertificateStructure.SubjectPublicKeyInfo.GetDerEncoded()
                .SequenceEqual(second.CertificateStructure.SubjectPublicKeyInfo.GetDerEncoded());
        }

        private bool CatalogKeyBelongsToUs(Guid kontoId, X509Certificate catalogCert)
        {
            if (_keyValidator.ValidateCertificateAgainstPrivateKeys(catalogCert))
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
            if (!_keyValidator.ValidateCertificateAgainstPrivateKeys(configuredCert))
            {
                throw new InvalidOperationException(
                    $"Configured public key for account {kontoId} does not match any configured private key.");
            }
        }
    }
}
