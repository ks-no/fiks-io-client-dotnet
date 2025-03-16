using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Models;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Send
{
    public class SendHandlerTests
    {
        private SendHandlerFixture _fixture;

        public SendHandlerTests()
        {
            _fixture = new SendHandlerFixture();
        }

        [Fact]
        public async Task CallsFiksIOSenderSend()
        {
            var sut = _fixture.CreateSut();

            var request = _fixture.DefaultRequest;

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(_ => _.Send(It.IsAny<MeldingSpesifikasjonApiModel>(), It.IsAny<Stream>()));
        }

        [Fact]
        public async Task CallsSendWithExpectedMessageSpecificationApiModelWithKlientHeaders()
        {
            var sut = _fixture.CreateSut();

            var request = new MeldingRequest(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                klientMeldingId: Guid.NewGuid(),
                klientKorrelasjonsId: Guid.NewGuid().ToString(),
                meldingType: "Meldingsprotokoll",
                ttl: TimeSpan.FromDays(2),
                headere: null,
                svarPaMelding: Guid.NewGuid());

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(_ => _.Send(
                It.Is<MeldingSpesifikasjonApiModel>(
                    model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                             model.SvarPaMelding == request.SvarPaMelding &&
                             model.AvsenderKontoId == request.AvsenderKontoId &&
                             model.MottakerKontoId == request.MottakerKontoId &&
                             model.Headere[MeldingBase.HeaderKlientMeldingId] == request.KlientMeldingId.ToString() &&
                             model.Headere[MeldingBase.HeaderKlientKorrelasjonsId] == request.KlientKorrelasjonsId),
                It.IsAny<Stream>()));
        }

        [Fact]
        public async Task CallsSendWithExpectedMessageSpecificationApiModelAndNullKlientHeaders()
        {
            var sut = _fixture.CreateSut();

            var request = new MeldingRequest(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                meldingType: "Meldingsprotokoll",
                ttl: TimeSpan.FromDays(2),
                headere: null,
                svarPaMelding: Guid.NewGuid());

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(_ => _.Send(
                It.Is<MeldingSpesifikasjonApiModel>(
                    model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                             model.SvarPaMelding == request.SvarPaMelding &&
                             model.AvsenderKontoId == request.AvsenderKontoId &&
                             model.MottakerKontoId == request.MottakerKontoId &&
                             !model.Headere.ContainsKey("klientMeldingId") &&
                             !model.Headere.ContainsKey("klientKorrelasjonsId")),
                It.IsAny<Stream>()));
        }

        [Fact]
        public async Task CallsEncrypterWithExpectedPrivateKey()
        {
            var expectedPublicKey = _fixture.CreateTestCertificate();
            var sut = _fixture.WithPublicKey(expectedPublicKey).CreateSut();
            var request = _fixture.DefaultRequest;
            var payload = new List<IPayload> {Mock.Of<IPayload>() };

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.AsicEncrypterMock.Verify(_ => _.Encrypt(expectedPublicKey, payload), Times.Once());
        }

        [Fact]
        public async Task GetsKeyFromCatalogHandler()
        {
            var sut = _fixture.CreateSut();
            var request = _fixture.DefaultRequest;
            var payload = new List<IPayload> {Mock.Of<IPayload>() };

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.CatalogHandlerMock.Verify(_ => _.GetPublicKey(request.MottakerKontoId), Times.Once());
        }
    }
}