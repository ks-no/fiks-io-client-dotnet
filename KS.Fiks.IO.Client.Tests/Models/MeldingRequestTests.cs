using System;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class MeldingRequestTests
    {
        [Fact]
        public void FromMeldingRequestToApiModelNullKlientModelIdShouldBeEmptyHeadere()
        {
            var result = new MeldingRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "meldingType").ToApiModel();
            result.Headere.Should().BeEmpty();
        }

        [Fact]
        public void FromMeldingRequestToApiModelWithKlientModelIdShouldBeInHeadere()
        {
            var klientMeldingId = Guid.NewGuid();
            var result = new MeldingRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "meldingType",
                klientMeldingId: klientMeldingId).ToApiModel();
            result.Headere.Should().NotBeEmpty();
            result.Headere.ContainsKey(MeldingBase.headerKlientMeldingId).Should().BeTrue();
            result.Headere.ContainsValue(klientMeldingId.ToString()).Should().BeTrue();
        }
    }
}