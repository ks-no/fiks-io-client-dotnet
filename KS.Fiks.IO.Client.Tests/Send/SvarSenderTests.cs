using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SvarSenderTests
    {
        private SvarSenderFixture _fixture;

        private sealed record MottattMeldingWrapper(
            MottattMelding MottattMelding,
            Guid MeldingId,
            Guid MottakerKonto,
            Guid AvsenderKonto,
            Guid? KlientMeldingId,
            Dictionary<string, string>? Headere);

        private MottattMeldingWrapper GetDefaultMottattMelding(Guid? klientMeldingId = null, Dictionary<string, string>? headere = null)
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();
            var melding = new MottattMelding(
                hasPayload: true,
                metadata: new MottattMeldingMetadata(
                    meldingId, "testType",
                    mottakerKonto,
                    avsenderKonto,
                    null,
                    TimeSpan.FromDays(1),
                    headere),
                streamProvider: _fixture.DefaultStreamProvider,
                decrypter: _fixture.DefaultDecrypter,
                fileWriter: _fixture.DefaultFileWriter);

            return new MottattMeldingWrapper(
                melding,
                meldingId,
                mottakerKonto,
                avsenderKonto,
                klientMeldingId ?? Guid.NewGuid(),
                headere);
        }

        public SvarSenderTests()
        {
            _fixture = new SvarSenderFixture();
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsender()
        {
            var melding = GetDefaultMottattMelding();
            var sut = _fixture.WithMottattMelding(melding.MottattMelding).CreateSut();

            await sut.Svar("testType").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == melding.AvsenderKonto && a.AvsenderKontoId == melding.MottakerKonto), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendsToAvsenderWithKlientMeldindId()
        {
            var klientMeldingId = Guid.NewGuid();
            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientMeldingId, klientMeldingId.ToString() }};
            var melding = GetDefaultMottattMelding(klientMeldingId, headere);
            var sut = _fixture.WithMottattMelding(melding.MottattMelding).CreateSut();

            await sut.Svar("testType", klientMeldingId).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == melding.AvsenderKonto && a.AvsenderKontoId == melding.MottakerKonto && a.KlientMeldingId == klientMeldingId), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendsToAvsenderWithKlientKorrelasjonsId()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();
            var klientKorrelasjonsId = Guid.NewGuid().ToString();

            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientKorrelasjonsId, klientKorrelasjonsId }};

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), headere), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType");

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto && a.KlientKorrelasjonsId == klientKorrelasjonsId), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendsToAvsenderWithKlientKorrelasjonsId_2()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();
            var klientKorrelasjonsId = Guid.NewGuid().ToString();

            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientKorrelasjonsId, klientKorrelasjonsId }};

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), headere), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType");

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto && a.KlientKorrelasjonsId == klientKorrelasjonsId), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsenderAndWithKlientMeldingId()
        {
            var melding = GetDefaultMottattMelding();
            var sut = _fixture.WithMottattMelding(melding.MottattMelding).CreateSut();

            await sut.Svar("testType", "my message", "message.txt", melding.KlientMeldingId).ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == melding.AvsenderKonto && a.AvsenderKontoId == melding.MottakerKonto), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsenderAndWithOptionalNullKlientMeldingId()
        {
            var melding = GetDefaultMottattMelding();
            var sut = _fixture.WithMottattMelding(melding.MottattMelding).CreateSut();

            await sut.Svar("testType", "my message", "message.txt").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == melding.AvsenderKonto && a.AvsenderKontoId == melding.MottakerKonto && a.KlientMeldingId == null), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendAllowsCancellation()
        {
            var melding = GetDefaultMottattMelding();
            var sut = _fixture.WithMottattMelding(melding.MottattMelding).CreateSut();

            var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await Assert.ThrowsAsync<TaskCanceledException>(
                async () => await sut.Svar(string.Empty, null, cts.Token).ConfigureAwait(false));
        }

        [Fact]
        public async Task SendsToAvsenderWithMottakerAsAvsenderAndWithOptionalNullKlientKorrelasjonsId()
        {
            var meldingId = Guid.NewGuid();
            var mottakerKonto = Guid.NewGuid();
            var avsenderKonto = Guid.NewGuid();

            var motattMelding = new MottattMelding(hasPayload: true, metadata: new MottattMeldingMetadata(meldingId, "testType", mottakerKonto, avsenderKonto, null, TimeSpan.FromDays(1), null), streamProvider: _fixture.DefaultStreamProvider, decrypter: _fixture.DefaultDecrypter, fileWriter: _fixture.DefaultFileWriter);

            var sut = _fixture.WithMottattMelding(motattMelding).CreateSut();

            await sut.Svar("testType", "my message", "message.txt").ConfigureAwait(false);

            _fixture.SendHandlerMock.Verify(_ => _.Send(It.Is<MeldingRequest>(a => a.MottakerKontoId == avsenderKonto && a.AvsenderKontoId == mottakerKonto && a.KlientMeldingId == null && a.KlientKorrelasjonsId == null), It.IsAny<IList<IPayload>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}