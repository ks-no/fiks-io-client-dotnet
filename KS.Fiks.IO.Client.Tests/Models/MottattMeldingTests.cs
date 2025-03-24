using System;
using System.Collections.Generic;
using KS.Fiks.IO.Client.Models;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class MottattMeldingTests
    {
        [Fact]
        public void TestKlientMeldingIdIsExtractedFromHeaderWhenGuid()
        {
            var klientMeldingId = Guid.NewGuid();
            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientMeldingId, klientMeldingId.ToString() }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere), null, null, null);

            mottattMelding.KlientMeldingId.ShouldBe(klientMeldingId);
        }

        [Fact]
        public void TestKlientMeldingIdIsGuidEmptyWhenNotAGuid()
        {
            var klientMeldingId = "dette er ikke en guid";
            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientMeldingId, klientMeldingId }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere), null, null, null);

            mottattMelding.KlientMeldingId.ShouldBe(Guid.Empty);
            mottattMelding.Headere.Values.ShouldContain(klientMeldingId);
        }

        [Fact]
        public void TestKlientMeldingIdIsGuidEmptyWhenWithoutHeadere()
        {
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, null), null, null, null);

            mottattMelding.KlientMeldingId.ShouldBeNull();
        }

        [Fact]
        public void TestResendtIsDefaultFalse()
        {
            var klientMeldingId = Guid.NewGuid();
            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientMeldingId, klientMeldingId.ToString() }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere), null, null, null);

            mottattMelding.Resendt.ShouldBeFalse();
        }

        [Fact]
        public void TestResendtIsTrueWhenSetToTrue()
        {
            var resendt = true;
            var klientMeldingId = Guid.NewGuid();
            var headere = new Dictionary<string, string>() {{ MeldingBase.HeaderKlientMeldingId, klientMeldingId.ToString() }};
            var mottattMelding = new MottattMelding(false,
                new MottattMeldingMetadata(Guid.NewGuid(), "meldingtype", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, headere, resendt), null, null, null);

            mottattMelding.Resendt.ShouldBeTrue();
        }
    }
}