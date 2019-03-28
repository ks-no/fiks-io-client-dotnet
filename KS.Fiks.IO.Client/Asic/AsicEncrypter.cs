using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Asic
{
    internal class AsicEncrypter : IAsicEncrypter
    {
        private readonly IAsiceBuilderFactory _asiceBuilderFactory;

        private readonly IEncryptionServiceFactory _encryptionServiceFactory;

        public AsicEncrypter(
            IAsiceBuilderFactory asiceBuilderFactory,
            IEncryptionServiceFactory encryptionServiceFactory)
        {
            _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();
            _encryptionServiceFactory = encryptionServiceFactory;
        }

        public Stream Encrypt(X509Certificate receiverCertificate, IEnumerable<IPayload> payloads)
        {
            if (!payloads.Any())
            {
                throw new ArgumentException("Payloads cannot be empty");
            }

            var zipStream = new MemoryStream();

            using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256, null))
            {
                foreach (var payload in payloads)
                {
                    asiceBuilder.AddFile(payload.Payload, payload.Filename);
                }
            }

            var encryptionService = _encryptionServiceFactory.Create(receiverCertificate);

            var outStream = new MemoryStream();
            using (var unencryptedStream = zipStream)
            {
                encryptionService.Encrypt(unencryptedStream, outStream);
            }

            return outStream;
        }
    }
}