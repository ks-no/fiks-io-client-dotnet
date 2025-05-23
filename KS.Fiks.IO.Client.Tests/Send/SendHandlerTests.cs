using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        public async Task Verify_Send_Message()
        {
            var sut = _fixture.CreateSut();

            var request = _fixture.DefaultRequest;

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(
                _ => _.Send(
                    It.IsAny<MeldingSpesifikasjonApiModel>(),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Verify_MessageSpecification_On_Send_With_ApiModel_With_KlientHeaders()
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

            _fixture.FiksIOSenderMock.Verify(
                _ => _.Send(
                It.Is<MeldingSpesifikasjonApiModel>(
                    model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                             model.SvarPaMelding == request.SvarPaMelding &&
                             model.AvsenderKontoId == request.AvsenderKontoId &&
                             model.MottakerKontoId == request.MottakerKontoId &&
                             model.Headere[MeldingBase.HeaderKlientMeldingId] == request.KlientMeldingId.ToString() &&
                             model.Headere[MeldingBase.HeaderKlientKorrelasjonsId] == request.KlientKorrelasjonsId),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Verify_MessageSpecification_On_Send_With_ApiModel_With_To_Equal_KlientHeaders()
        {
            var sut = _fixture.CreateSut();
            var klientKorrelasjonsId = Guid.NewGuid().ToString();
            var klientMeldingId = Guid.NewGuid();

            var request = new MeldingRequest(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                klientMeldingId: klientMeldingId,
                klientKorrelasjonsId: klientKorrelasjonsId,
                meldingType: "Meldingsprotokoll",
                ttl: TimeSpan.FromDays(2),
                headere: new Dictionary<string, string>() {{MeldingBase.HeaderKlientKorrelasjonsId, klientKorrelasjonsId}, {MeldingBase.HeaderKlientMeldingId, klientMeldingId.ToString() }},
                svarPaMelding: Guid.NewGuid());

            var payload = new List<IPayload>();

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.FiksIOSenderMock.Verify(
                _ => _.Send(
                    It.Is<MeldingSpesifikasjonApiModel>(
                        model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                                 model.SvarPaMelding == request.SvarPaMelding &&
                                 model.AvsenderKontoId == request.AvsenderKontoId &&
                                 model.MottakerKontoId == request.MottakerKontoId &&
                                 model.Headere[MeldingBase.HeaderKlientMeldingId] == request.KlientMeldingId.ToString() &&
                                 model.Headere[MeldingBase.HeaderKlientKorrelasjonsId] == request.KlientKorrelasjonsId),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Verify_Exception_Returned_When_Send_With_Two_Different_KlientKorrelasjonsId_Set()
        {
            var sut = _fixture.CreateSut();
            var klientKorrelasjonsId1 = Guid.NewGuid().ToString();
            var klientKorrelasjonsId2 = Guid.NewGuid().ToString();

            var request = new MeldingRequest(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                klientKorrelasjonsId: klientKorrelasjonsId1,
                meldingType: "Meldingsprotokoll",
                ttl: TimeSpan.FromDays(2),
                headere: new Dictionary<string, string>() {{MeldingBase.HeaderKlientKorrelasjonsId, klientKorrelasjonsId2}},
                svarPaMelding: Guid.NewGuid());

            var payload = new List<IPayload>();
            await Assert.ThrowsAsync<ArgumentException>(() => sut.Send(request, payload));
        }

        [Fact]
        public async Task Verify_Exception_Returned_When_Send_With_Two_Different_KlientMeldingId_Set()
        {
            var sut = _fixture.CreateSut();
            var klientMeldingId1 = Guid.NewGuid();
            var klientMeldingId2 = Guid.NewGuid();

            var request = new MeldingRequest(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                klientMeldingId: klientMeldingId1,
                meldingType: "Meldingsprotokoll",
                ttl: TimeSpan.FromDays(2),
                headere: new Dictionary<string, string>() {{MeldingBase.HeaderKlientMeldingId, klientMeldingId2.ToString()}},
                svarPaMelding: Guid.NewGuid());

            var payload = new List<IPayload>();
            await Assert.ThrowsAsync<ArgumentException>(() => sut.Send(request, payload));
        }


        [Fact]
        public async Task Verify_MessageSpecification_On_Send_With_ApiModel_With_Null_KlientHeaders()
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

            _fixture.FiksIOSenderMock.Verify(
                _ => _.Send(
                It.Is<MeldingSpesifikasjonApiModel>(
                    model => model.Ttl == (long)request.Ttl.TotalMilliseconds &&
                             model.SvarPaMelding == request.SvarPaMelding &&
                             model.AvsenderKontoId == request.AvsenderKontoId &&
                             model.MottakerKontoId == request.MottakerKontoId &&
                             !model.Headere.ContainsKey("klientMeldingId") &&
                             !model.Headere.ContainsKey("klientKorrelasjonsId")),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Verify_Encrypter_With_Expected_PrivateKey()
        {
            var expectedPublicKey = _fixture.CreateTestCertificate();
            var sut = _fixture.WithPublicKey(expectedPublicKey).CreateSut();
            var request = _fixture.DefaultRequest;
            var payload = new List<IPayload> {Mock.Of<IPayload>() };

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.AsicEncrypterMock.Verify(_ => _.Encrypt(expectedPublicKey, payload), Times.Once());
        }

        [Fact]
        public async Task Verify_GetKey_From_CatalogHandler()
        {
            var sut = _fixture.CreateSut();
            var request = _fixture.DefaultRequest;
            var payload = new List<IPayload> {Mock.Of<IPayload>() };

            await sut.Send(request, payload).ConfigureAwait(false);

            _fixture.CatalogHandlerMock.Verify(_ => _.GetPublicKey(request.MottakerKontoId), Times.Once());
        }
    }
}