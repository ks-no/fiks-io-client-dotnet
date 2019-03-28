using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Catalog
{
    internal interface ICatalogHandler
    {
        Task<Account> Lookup(LookupRequest request);

        Task<X509Certificate> GetPublicKey(Guid receiverAccountId);
    }
}