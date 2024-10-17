using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Configuration
{
    public class FiksIOConfigurationTests
    {
        [Fact]
        public void WorksWithNullApiConfigurationConstructor()
        {
            var result = new FiksIOConfiguration(
                new KontoConfiguration(Guid.NewGuid(),"string"),
                new IntegrasjonConfiguration(Guid.NewGuid(),"password"),
                new MaskinportenClientConfiguration("aud", "tokenedm", "issuer",100, Mock.Of<X509Certificate2>()),
                null);
        }
    }
}