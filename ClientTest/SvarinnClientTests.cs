using NFluent;
using NUnit.Framework;

namespace Ks.Fiks.Svarinn.ClientTest
{
    public class TestBase
    {
        protected SvarinnClientFixture Fixture;
        
        [SetUp]
        public void Setup()
        {
            Fixture = new SvarinnClientFixture();
        }

    }

    [TestFixture]
    public class KontoId : TestBase
    {
        [Test]
        public void ReturnsAString()
        {
            var sut = Fixture.CreateSut();
            Check.That(sut.GetKontoId()).IsInstanceOf<string>();
        }
    }
}