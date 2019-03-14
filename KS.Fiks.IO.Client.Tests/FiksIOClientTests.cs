using System;
using System.Threading.Tasks;
using FluentAssertions;
using KS.Fiks.IO.Client.Models;
using Xunit;

namespace KS.Fiks.IO.Client.Tests
{
    public class FiksIOClientTests
    {
        private FiksIOClientFixture _fixture;

        public FiksIOClientTests()
        {
            _fixture = new FiksIOClientFixture();
        }

        [Fact]
        public void HasExpectedAccountId()
        {
            var expectedAccountId = "TestId";
            var sut = _fixture.WithAccountId(expectedAccountId).CreateSut();
            var actualAccountId = sut.AccountId;
            actualAccountId.Should().Be(expectedAccountId);
        }

        [Fact]
        public async Task LookupReturnsExpectedAccount()
        {
            var expectedAccount = new Account
            {
                AccountId = Guid.NewGuid(),
                AccountName = "testName",
                IsValidSender = true,
                IsValidReceiver = false,
                OrgId = Guid.NewGuid(),
                OrgName = "testOrgName"
            };
            var lookup = new LookupRequest();
            var sut = _fixture.WithLookupAccount(expectedAccount).CreateSut();
            var actualAccount = await sut.Lookup(lookup).ConfigureAwait(false);
            actualAccount.Should().Be(expectedAccount);
        }

        [Fact]
        public async Task LookupCallsCatalogHandlerWithExpectedLookup()
        {
            var lookup = new LookupRequest
            {
                Identifier = "testIdentifier",
                AccessLevel = 3,
                MessageType = "testType"
            };
            var sut = _fixture.WithLookupAccount(new Account()).CreateSut();
            var actualAccount = await sut.Lookup(lookup).ConfigureAwait(false);

            _fixture.CatalogHandlerMock.Verify(_ => _.Lookup(lookup));
        }
    }
}