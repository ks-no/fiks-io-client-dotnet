using FluentAssertions;
using Xunit;

namespace KS.Fiks.IO.Client.Tests
{
    public class FiksIOClientTests
    {
        private FiksIOClientFixture _fixture;

        public FiksIOClientTests()
        {
            _fixture = new FiksIOClientFixture();
        }

        [Fact]
        public void ReturnsAString()
        {
            var sut = _fixture.CreateSut();
            var kontoId = sut.GetKontoId();
            kontoId.Should().BeOfType<string>();
        }
    }
}