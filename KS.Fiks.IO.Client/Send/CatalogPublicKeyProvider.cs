using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Catalog;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Send
{
    internal class CatalogPublicKeyProvider : IPublicKeyProvider
    {
        private readonly ICatalogHandler _catalogHandler;

        public CatalogPublicKeyProvider(ICatalogHandler catalogHandler)
        {
            _catalogHandler = catalogHandler;
        }

        public Task<X509Certificate> GetPublicKey(Guid receiverAccountId)
        {
            return _catalogHandler.GetPublicKey(receiverAccountId);
        }
    }
}