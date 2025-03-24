using System;
using KS.Fiks.IO.Client.Models;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Models
{
    public class MeldingRequestTests
    {
        [Fact]
        public void FromMeldingRequestToApiModelShouldBeEmptyHeadere()
        {
            var result = new MeldingRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "meldingType").ToApiModel();
            result.Headere.ShouldBeEmpty();
        }

        [Fact]
        public void FromMeldingRequestToApiModelWithKlientMeldingIdShouldBeInHeadere()
        {
            var klientMeldingId = Guid.NewGuid();
            var result = new MeldingRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "meldingType",
                klientMeldingId: klientMeldingId).ToApiModel();
            result.Headere.ShouldNotBeEmpty();
            result.Headere.ContainsKey(MeldingBase.HeaderKlientMeldingId).ShouldBeTrue();
            result.Headere.ContainsValue(klientMeldingId.ToString()).ShouldBeTrue();
        }

        [Fact]
        public void FromMeldingRequestToApiModelWithKlientKorrelasjonsIdShouldBeInHeadere()
        {
            var klientKorrelasjonsId = Guid.NewGuid().ToString();
            var result = new MeldingRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "meldingType",
                klientKorrelasjonsId: klientKorrelasjonsId).ToApiModel();
            result.Headere.ShouldNotBeEmpty();
            result.Headere.ContainsKey(MeldingBase.HeaderKlientKorrelasjonsId).ShouldBeTrue();
            result.Headere.ContainsValue(klientKorrelasjonsId).ShouldBeTrue();
        }
    }
}