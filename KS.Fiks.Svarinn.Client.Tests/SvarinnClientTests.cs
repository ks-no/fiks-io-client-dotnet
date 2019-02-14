using Xunit;
using FluentAssertions;
    
namespace Ks.Fiks.Svarinn.ClientTest
{
    public class KontoId
    {
        private SvarinnClientFixture _fixture;

        public KontoId()
        {
            _fixture = new SvarinnClientFixture();
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