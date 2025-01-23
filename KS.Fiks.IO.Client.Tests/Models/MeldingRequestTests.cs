using System;
using KS.Fiks.IO.Client.Models;
using Shouldly;
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
            result.Headere.ShouldBeEmpty();
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
            result.Headere.ShouldNotBeEmpty();
            result.Headere.ContainsKey(MeldingBase.headerKlientMeldingId).ShouldBeTrue();
            result.Headere.ContainsValue(klientMeldingId.ToString()).ShouldBeTrue();
        }
    }
}