using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Asic
{
    internal class AsicEncrypter : IAsicEncrypter
    {
        private readonly IAsiceBuilderFactory _asiceBuilderFactory;

        private readonly IEncryptionServiceFactory _encryptionServiceFactory;

        private readonly PreloadedCertificateHolder _asiceCertificateHolder;

        public AsicEncrypter(
            IAsiceBuilderFactory asiceBuilderFactory,
            IEncryptionServiceFactory encryptionServiceFactory,
            PreloadedCertificateHolder preloadedCertificateHolder)
        {
            _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();
            _encryptionServiceFactory = encryptionServiceFactory;
            _asiceCertificateHolder = preloadedCertificateHolder;
        }

        public Stream Encrypt(X509Certificate receiverCertificate, IList<IPayload> payloads)
        {
            ThrowIfEmpty(payloads);
            return ZipAndEncrypt(receiverCertificate, payloads);
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

        private Stream ZipAndEncrypt(X509Certificate receiverCertificate, IEnumerable<IPayload> payloads)
        {
            var outStream = new MemoryStream();
            var encryptionService = _encryptionServiceFactory.Create(receiverCertificate);
            using (var zipStream = new MemoryStream())
            {
                using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256, _asiceCertificateHolder))
                {
                    foreach (var payload in payloads)
                    {
                        payload.Payload.Seek(0, SeekOrigin.Begin);
                        asiceBuilder.AddFile(payload.Payload, payload.Filename);
                        asiceBuilder.Build();
                    }
                }

                zipStream.Seek(0, SeekOrigin.Begin);
                encryptionService.Encrypt(zipStream, outStream);
            }

            return outStream;
        }
    }
}