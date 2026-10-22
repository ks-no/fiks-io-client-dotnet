using System;
using System.Collections.Generic;
using System.Linq;

namespace KS.Fiks.IO.Client.Configuration
{
    public class KontoConfiguration
    {
        public KontoConfiguration(Guid kontoId, string privatNokkel)
            : this(kontoId, new[] { privatNokkel }, null)
        {
        }

        public KontoConfiguration(Guid kontoId, string privatNokkel, string offentligNokkel)
            : this(kontoId, new[] { privatNokkel }, offentligNokkel)
        {
        }

        public KontoConfiguration(Guid kontoId, IEnumerable<string> privatNokler)
            : this(kontoId, privatNokler, null)
        {
        }

        public KontoConfiguration(Guid kontoId, IEnumerable<string> privatNokler, string offentligNokkel)
        {
            KontoId = kontoId;
            if (privatNokler == null)
            {
                throw new ArgumentNullException(nameof(privatNokler));
            }

            PrivatNokler = privatNokler.ToList();

            if (!PrivatNokler.Any())
            {
                throw new ArgumentException("Must provide at least one private key.", nameof(privatNokler));
            }

            if (PrivatNokler.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Private keys must not be null or whitespace.", nameof(privatNokler));
            }

            OffentligNokkel = offentligNokkel;
        }

        public Guid KontoId { get; }

        /// <summary>
        /// Private keys matching the public key registered for the account in the Fiks configuration. Used to decrypt incoming messages.
        /// Several private keys can be added to support key rotation.
        /// </summary>
        public List<string> PrivatNokler { get; }

        /// <summary>
        /// Optional public key (PEM-encoded X.509 certificate) for the account. When set, the client uploads
        /// the key to the Fiks-IO catalog on startup if the key is missing or outdated.
        /// </summary>
        public string OffentligNokkel { get; }
    }
}