using NFluent;
using NUnit.Framework;

namespace SvarinnClient.Tests
{
    public class TestBase
    {
        protected SvarinnKlientFixture Fixture;
        
        [SetUp]
        public void Setup()
        {
            Fixture = new SvarinnKlientFixture();
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