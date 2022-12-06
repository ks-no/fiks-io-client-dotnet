using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.ASiC_E;
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
        private readonly ICertificateHolder _asiceSigningCertificateHolder;

        public AsicEncrypter(
            IAsiceBuilderFactory asiceBuilderFactory,
            IEncryptionServiceFactory encryptionServiceFactory,
            ICertificateHolder signingCertificateHolder)
        {
            _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();
            _encryptionServiceFactory = encryptionServiceFactory;
            _asiceSigningCertificateHolder = signingCertificateHolder;
        }

        public AsicEncrypter(
            IAsiceBuilderFactory asiceBuilderFactory,
            IEncryptionServiceFactory encryptionServiceFactory)
        {
            _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();
            _encryptionServiceFactory = encryptionServiceFactory;
            _asiceSigningCertificateHolder = null;
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
                if (_asiceSigningCertificateHolder == null)
                {
                    BuildAsiceWithoutSigning(payloads, zipStream);
                }
                else
                {
                    BuildAsiceWithSigning(payloads, zipStream);
                }

                zipStream.Seek(0, SeekOrigin.Begin);
                encryptionService.Encrypt(zipStream, outStream);
            }

            return outStream;
        }

        private void BuildAsiceWithoutSigning(IEnumerable<IPayload> payloads, MemoryStream zipStream)
        {
            using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256))
            {
                BuildAsice(payloads, asiceBuilder);
            }
        }

        private void BuildAsiceWithSigning(IEnumerable<IPayload> payloads, MemoryStream zipStream)
        {
            using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256, _asiceSigningCertificateHolder))
            {
               BuildAsice(payloads, asiceBuilder);
            }
        }

        private static void BuildAsice(IEnumerable<IPayload> payloads, IAsiceBuilder<AsiceArchive> asiceBuilder)
        {
            foreach (var payload in payloads)
            {
                payload.Payload.Seek(0, SeekOrigin.Begin);
                asiceBuilder.AddFile(payload.Payload, payload.Filename);
                asiceBuilder.Build();
            }
        }
    }
}