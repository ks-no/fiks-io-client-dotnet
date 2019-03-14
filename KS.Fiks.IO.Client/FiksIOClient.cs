using System.Threading.Tasks;
using Ks.Fiks.Maskinporten.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Utility;

namespace KS.Fiks.IO.Client
{
    public class FiksIOClient : IFiksIOClient
    {
        private readonly ICatalogHandler _catalogHandler;

        private readonly IMaskinportenClient _maskinportenClient;

        public FiksIOClient(FiksIOConfiguration configuration, ICatalogHandler catalogHandler = null, IMaskinportenClient maskinportenClient = null)
        {
            configuration = ConfigurationNormalizer.SetDefaultValues(configuration);

            AccountId = configuration.AccountConfiguration.AccountId;
            _maskinportenClient = maskinportenClient ?? new MaskinportenClient(configuration.MaskinportenConfiguration);
            _catalogHandler = catalogHandler ?? new CatalogHandler(configuration, _maskinportenClient);
        }

        public string AccountId { get; }

        public async Task<Account> Lookup(LookupRequest request)
        {
            return await _catalogHandler.Lookup(request).ConfigureAwait(false);
        }
    }
}