using System.IO;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.Configuration;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public class AsicSigningCertificateHolderFactoryTests
    {
        [Fact]
        public void CreateCertificateHolder_With_ReadOnly_Files()
        {
            File.SetAttributes("fiks_demo_private.pem", FileAttributes.ReadOnly);
            File.SetAttributes("fiks_demo_public.pem", FileAttributes.ReadOnly);
            var asiceSigningConfiguration =
                new AsiceSigningConfiguration("fiks_demo_public.pem", "fiks_demo_private.pem");
            AsicSigningCertificateHolderFactory.Create(asiceSigningConfiguration);
        }
    }
}