namespace KS.Fiks.IO.Client.Configuration
{
    public class AccountConfiguration
    {
        public AccountConfiguration(string accountId, string privateKey)
        {
            AccountId = accountId;
            PrivateKey = privateKey;
        }

        public string AccountId { get; }

        public string PrivateKey { get; }
    }
}