using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KS.Fiks.ASiC_E;
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
            // var zipStream = CreateZipStream(payloads);
            // var outStream = EncryptStream(zipStream, receiverCertificate);
            // zipStream.Dispose();
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

        private Stream CreateZipStream(IEnumerable<IPayload> payloads)
        {
            var zipStream = new MemoryStream();
            using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256))
            {
                foreach (var payload in payloads)
                {
                    payload.Payload.Seek(0, SeekOrigin.Begin);
                    asiceBuilder.AddFile(payload.Payload, payload.Filename);
                    asiceBuilder.Build();
                }
            }
            return zipStream;
        }

        private Stream EncryptStream(Stream zipStream, X509Certificate certificate)
        {
            var encryptionService = _encryptionServiceFactory.Create(certificate);
            var outStream = new MemoryStream();
            encryptionService.Encrypt(zipStream, outStream);

            return outStream;
        }

        private Stream ZipAndEncrypt(X509Certificate certificate, IEnumerable<IPayload> payloads)
        {
            var zipStream = new MemoryStream();
            var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256);
            try
            {
                foreach (var payload in payloads)
                {
                    payload.Payload.Seek(0, SeekOrigin.Begin);
                    asiceBuilder.AddFile(payload.Payload, payload.Filename);
                    asiceBuilder.Build();
                }
            }
            catch (Exception e)
            {
                zipStream.Dispose();
                throw e;
            }
            finally
            {
                asiceBuilder.Dispose();
            }

            var outStream = new MemoryStream();
            try
            {
                var encryptionService = _encryptionServiceFactory.Create(certificate);
                encryptionService.Encrypt(zipStream, outStream);
            }
            finally
            {
                zipStream.Dispose();
            }

            return outStream;
        }
    }
}