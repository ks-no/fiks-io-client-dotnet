using System;
using System.Collections.Generic;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Send.Client.Models;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class SendtMeldingTests
    {
        [Fact]
        public void FromSentMessageApiModelWithNullHeadere()
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

            result.KlientMeldingId.Should().BeNull();
        }

        [Fact]
        public void FromSentMessageApiModelWithEmptyHeadereDictionaryKlientMeldingShouldBeNull()
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

            result.KlientMeldingId.Should().BeNull();
        }

        [Fact]
        public void FromSentMessageApiModelWithKlientMeldingId()
        {
            var klientMeldingId = Guid.NewGuid();
            var result = SendtMelding.FromSentMessageApiModel(
                new SendtMeldingApiModel()
                {
                    SvarPaMelding = Guid.NewGuid(),
                    AvsenderKontoId = Guid.NewGuid(),
                    DokumentlagerId = Guid.NewGuid(),
                    Headere = new Dictionary<string, string>() {{ MeldingBase.headerKlientMeldingId, klientMeldingId.ToString() }},
                    MeldingId = Guid.NewGuid(),
                    MeldingType = "EnMeldingType",
                    MottakerKontoId = Guid.NewGuid(),
                    Ttl = 1000L
                });

            result.KlientMeldingId.Should().NotBeNull();
            result.KlientMeldingId.Should().Be(klientMeldingId.ToString());
        }
    }
}