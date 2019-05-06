using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class AccountConfiguration
    {
        public AccountConfiguration(Guid accountId, string privateKey)
        {
            AccountId = accountId;
            PrivateKey = privateKey;
        }

        public Guid AccountId { get; }

        public string PrivateKey { get; }
    }
}