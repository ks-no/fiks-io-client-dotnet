using System.IO;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Model;

namespace KS.Fiks.IO.Client.Asic
{
    public interface IAsiceBuilderFactory
    {
        IAsiceBuilder<AsiceArchive> GetBuilder(
            Stream outStream,
            MessageDigestAlgorithm messageDigestAlgorithm);
    }
}