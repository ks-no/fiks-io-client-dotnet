using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SvarSenderTests
    {
        private SvarSenderFixture _fixture;

        public SvarSenderTests()
        {
            _fixture = new SvarSenderFixture();
        }

        [Fact]
        public async Task SendsToAvsenderWithMotakerAsAvsender()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();

            var motattMelding = new MottattMelding(true, new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1)), _fixture.DefaultStreamProvider, _fixture.DefaultDecrypter, _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto), It.IsAny<IList<IPayload>>()));

        }
    }
}