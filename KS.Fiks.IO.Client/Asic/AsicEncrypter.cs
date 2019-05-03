using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.ASiC_E.Model;
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

        public Stream Encrypt(X509Certificate receiverCertificate, IList<IPayload> payloads)
        {
            ThrowIfEmpty(payloads);

            var zipStream = CreateZipStream(payloads);

            var outStream = EncryptStream(zipStream, receiverCertificate);

            return outStream;
        }

        private void ThrowIfEmpty(IEnumerable<IPayload> payloads)
        {
            if (payloads == null)
            {
                throw new ArgumentNullException(nameof(payloads));
            }

            if (!payloads.Any())
            {
                throw new ArgumentException("Payloads cannot be empty");
            }
        }

        private Stream CreateZipStream(IEnumerable<IPayload> payloads)
        {
            byte[] zippedBytes;
            
            using (var zipStream = new MemoryStream())
            {
                using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256))
                {
                    foreach (var payload in payloads)
                    {
                        payload.Payload.Seek(0, SeekOrigin.Begin);
                        asiceBuilder.AddFile(payload.Payload, payload.Filename);
                        asiceBuilder.Build();
                    }
                }
                
                zippedBytes = zipStream.ToArray();
            }

            return new MemoryStream(zippedBytes);
        }

        private Stream EncryptStream(Stream zipStream, X509Certificate certificate)
        {
            var encryptionService = _encryptionServiceFactory.Create(certificate);
            var outStream = new MemoryStream();
            encryptionService.Encrypt(zipStream, outStream);

            return outStream;
        }
    }
}