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

        private Stream ZipAndEncrypt(X509Certificate certificate, IEnumerable<IPayload> payloads)
        {
            var zipStream = new MemoryStream();
            var outStream = new MemoryStream();

            try
            {
                using(var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256)) 
                {
                    foreach (var payload in payloads)
                    {
                        payload.Payload.Seek(0, SeekOrigin.Begin);
                        asiceBuilder.AddFile(payload.Payload, payload.Filename);
                        asiceBuilder.Build();
                    }
                }
                //TODO This is hopefully an unnecessary copy to a new stream here? Cannot use zipstream since asiceBuilder needs to get disposed in order to create a manifest and then seems to close the stream too
                var extraStream = new MemoryStream(zipStream.ToArray());
                var encryptionService = _encryptionServiceFactory.Create(certificate);

                encryptionService.Encrypt(extraStream, outStream);
            }
            catch (Exception e)
            {
                zipStream.Dispose();
                outStream.Dispose();
                throw e;
            }

            return outStream;
        }
    }
}