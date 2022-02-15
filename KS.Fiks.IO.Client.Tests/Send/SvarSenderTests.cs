using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
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
        public async Task SendsToAvsenderWithMottakerAsAvsender()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), null), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto), It.IsAny<IList<IPayload>>()));
        }

        [Fact]
        public async Task SendsToAvsenderWithKlientMeldindId()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();
            var klientMeldingId = Guid.NewGuid();

            var headere = new Dictionary<string, string>() {{ MeldingBase.headerKlientMeldingId, klientMeldingId.ToString() }};

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), headere), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType", klientMeldingId).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto && a.KlientMeldingId == klientMeldingId), It.IsAny<IList<IPayload>>()));
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsenderAndWithKlientMeldingId()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();
            var klientMeldingId = Guid.NewGuid();

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), null), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType", "my message", "message.txt", klientMeldingId).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto), It.IsAny<IList<IPayload>>()));
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsenderAndWithOptionalNullKlientMeldingId()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), null), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType", "my message", "message.txt").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto && a.KlientMeldingId == null), It.IsAny<IList<IPayload>>()));
        }
    }
}