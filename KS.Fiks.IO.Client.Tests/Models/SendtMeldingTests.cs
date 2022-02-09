using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Send.Client.Models;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class SendtMeldingTests
    {
        [Fact]
        public async void FromSentMessageApiModelWithNullHeadere()
        {
            var result = SendtMelding.FromSentMessageApiModel(
                new SendtMeldingApiModel()
                {
                    SvarPaMelding = Guid.NewGuid(),
                    AvsenderKontoId = Guid.NewGuid(),
                    DokumentlagerId = Guid.NewGuid(),
                    Headere = null,
                    MeldingId = Guid.NewGuid(),
                    MeldingType = "EnMeldingType",
                    MottakerKontoId = Guid.NewGuid(),
                    Ttl = 1000L
                });
        }

        [Fact]
        public async void FromSentMessageApiModelWithEmptyHeadereDictionary()
        {
            var result = SendtMelding.FromSentMessageApiModel(
                new SendtMeldingApiModel()
                {
                    SvarPaMelding = Guid.NewGuid(),
                    AvsenderKontoId = Guid.NewGuid(),
                    DokumentlagerId = Guid.NewGuid(),
                    Headere = new Dictionary<string, string>(),
                    MeldingId = Guid.NewGuid(),
                    MeldingType = "EnMeldingType",
                    MottakerKontoId = Guid.NewGuid(),
                    Ttl = 1000L
                });
        }

        [Fact]
        public async void FromSentMessageApiModelWithKlientMeldingId()
        {
            var result = SendtMelding.FromSentMessageApiModel(
                new SendtMeldingApiModel()
                {
                    SvarPaMelding = Guid.NewGuid(),
                    AvsenderKontoId = Guid.NewGuid(),
                    DokumentlagerId = Guid.NewGuid(),
                    Headere = new Dictionary<string, string>() {{MeldingBase.headerKlientMeldingId, "enGuidIsh"}},
                    MeldingId = Guid.NewGuid(),
                    MeldingType = "EnMeldingType",
                    MottakerKontoId = Guid.NewGuid(),
                    Ttl = 1000L
                });
        }
    }
}