using System.IO;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsiceBuilderFactory
    {
        IAsiceBuilder<AsiceArchive> GetBuilder(
            Stream outStream,
            MessageDigestAlgorithm messageDigestAlgorithm);

        IAsiceBuilder<AsiceArchive> GetBuilder(
            Stream outStream,
            MessageDigestAlgorithm messageDigestAlgorithm,
            ICertificateHolder certificateHolder);
    }
}