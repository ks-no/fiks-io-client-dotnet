using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Catalog;
using Microsoft.Extensions.Logging;

namespace KS.Fiks.IO.Client
{
    internal class KeyValidatorHandler
    {
        private readonly ICatalogHandler _catalogHandler;
        private readonly KontoConfiguration _kontoConfiguration;
        private readonly ILogger<KeyValidatorHandler> _logger;

        public KeyValidatorHandler(ICatalogHandler catalogHandler, KontoConfiguration kontoConfiguration, ILoggerFactory loggerFactory = null)
        {
            _catalogHandler = catalogHandler;
            _kontoConfiguration = kontoConfiguration;
            _logger = loggerFactory?.CreateLogger<KeyValidatorHandler>();
        }

        /// <summary>
        /// Checks that the public key registered in the Fiks-IO catalog for the konto
        /// matches the private key configured in this client by encrypting random bytes
        /// with the public key and decrypting with the private key.
        /// </summary>
        /// <returns>True if the keys match, false if there is a mismatch or decryption fails.</returns>
        public async Task<bool> ValidatePublicKeyAgainstPrivateKeyAsync()
        {
            if (_kontoConfiguration.PrivatNokler == null || _kontoConfiguration.PrivatNokler.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot validate key pair for konto {_kontoConfiguration.KontoId}: no private keys are configured in KontoConfiguration.");
            }

            var certificate = await _catalogHandler.GetPublicKey(_kontoConfiguration.KontoId).ConfigureAwait(false);

            var randomBytes = new byte[256];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            try
            {
                using (var plainStream = new MemoryStream(randomBytes))
                using (var encryptedStream = new MemoryStream())
                {
                    EncryptionService.Create(certificate).Encrypt(plainStream, encryptedStream);

                    encryptedStream.Position = 0;
                    using (var decryptedStream = DecryptionService.Create(_kontoConfiguration.PrivatNokler[0]).Decrypt(encryptedStream))
                    using (var resultStream = new MemoryStream())
                    {
                        decryptedStream.CopyTo(resultStream);
                        return resultStream.ToArray().SequenceEqual(randomBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "Validation failed for account {KontoId}. The private key does not match the public key from Fiks-IO catalog api.",
                    _kontoConfiguration.KontoId);
                return false;
            }
        }
    }
}
