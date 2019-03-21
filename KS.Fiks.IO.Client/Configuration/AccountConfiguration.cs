namespace KS.Fiks.IO.Client.Configuration
{
    public class AccountConfiguration
    {
        public AccountConfiguration(string accountId)
        {
            AccountId = accountId;
        }

        public string AccountId { get; }
    }
}