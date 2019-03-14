using System;

namespace KS.Fiks.IO.Client.Models
{
    public class Account
    {
        public Guid OrgId { get; set; }

        public string OrgName { get; set; }

        public Guid AccountId { get; set; }

        public string AccountName { get; set; }

        public bool IsValidSender { get; set; }

        public bool IsValidReceiver { get; set; }

        internal static Account FromAccountResponse(AccountResponse accountResponse)
        {
            return new Account
            {
                OrgId = accountResponse.OrgId,
                OrgName = accountResponse.OrgName,
                AccountId = accountResponse.AccountId,
                AccountName = accountResponse.AccountName,
                IsValidSender = accountResponse.Status.ValidSender,
                IsValidReceiver = accountResponse.Status.ValidReceiver
            };
        }
    }
}