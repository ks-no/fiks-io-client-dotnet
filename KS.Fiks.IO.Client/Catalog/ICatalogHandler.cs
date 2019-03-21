using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Catalog
{
    internal interface ICatalogHandler
    {
        Task<Account> Lookup(LookupRequest request);

        Task<string> GetPublicKey(Guid receiverAccountId);
    }
}