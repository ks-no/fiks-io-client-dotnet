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

        private readonly ICryptoService _cryptoService;

        public AsicEncrypter(IAsiceBuilderFactory asiceBuilderFactory, ICryptoService cryptoService)
        {
            _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();
            _cryptoService = cryptoService;
        }

        public Stream Encrypt(X509Certificate receiverCertificate, IEnumerable<IPayload> payloads)
        {
            if (!payloads.Any())
            {
                throw new ArgumentException("Payloads cannot be empty");
            }

            Stream zipStream = new MemoryStream();

            using (var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256, null))
            {
                foreach (var payload in payloads)
                {
                    asiceBuilder.AddFile(payload.Payload, payload.Filename);
                }
            }

            Stream encryptedStream = new MemoryStream();
            _cryptoService.Encrypt(zipStream, encryptedStream);

            return encryptedStream;
        }
    }
}