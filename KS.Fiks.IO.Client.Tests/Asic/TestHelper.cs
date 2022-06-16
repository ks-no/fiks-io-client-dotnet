using System.IO;
using System.Linq;

namespace KS.Fiks.IO.Client.Tests.Asic
{
    public static class TestHelper
    {
        public static bool StreamEquals(Stream stream1, Stream stream2)
        {
            return ReadStream(stream1).SequenceEqual(ReadStream(stream2));
        }

        private static byte[] ReadStream(Stream stream)
        {
            using var reader = new BinaryReader(stream);
            var outBytes = reader.ReadBytes((int) stream.Length);
            stream.Seek(0L, SeekOrigin.Begin);
            return outBytes;
        }
    }
}