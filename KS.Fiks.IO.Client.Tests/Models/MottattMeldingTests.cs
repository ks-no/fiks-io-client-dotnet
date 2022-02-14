using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Send.Client.Models;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class MottattMeldingTests
    {
        [Fact]
        public void TestKlientMeldingIdIsExtractedFromHeaderWhenGuid()
        {
            var klientMeldingId = Guid.NewGuid();
            var headere = new Dictionary<string, string>() {{ MeldingBase.headerKlientMeldingId, klientMeldingId.ToString() }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere), null, null, null);

            mottattMelding.KlientMeldingId.Should().Be(klientMeldingId);
        }

        [Fact]
        public void TestKlientMeldingIdIsGuidEmptyWhenNotAGuid()
        {
            var klientMeldingId = "dette er ikke en guid";
            var headere = new Dictionary<string, string>() {{ MeldingBase.headerKlientMeldingId, klientMeldingId }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere), null, null, null);

            mottattMelding.KlientMeldingId.Should().Be(Guid.Empty);
            mottattMelding.Headere.Values.Should().Contain(klientMeldingId);
        }

        [Fact]
        public void TestKlientMeldingIdIsGuidEmptyWhenWithoutHeadere()
        {
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, null), null, null, null);

            mottattMelding.KlientMeldingId.Should().Be(Guid.Empty);
        }
    }
}