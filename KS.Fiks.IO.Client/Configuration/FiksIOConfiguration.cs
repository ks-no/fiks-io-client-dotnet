using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public FiksIOConfiguration(
            AccountConfiguration accountConfiguration,
            FiksIntegrationConfiguration fiksIntegrationConfiguration,
            MaskinportenClientConfiguration maskinportenConfiguration,
            CatalogConfiguration catalogConfiguration = null,
            FiksApiConfiguration fiksApiConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null
        )
        {
            AccountConfiguration = accountConfiguration;
            FiksIntegrationConfiguration = fiksIntegrationConfiguration;
            MaskinportenConfiguration = maskinportenConfiguration;
            FiksApiConfiguration = fiksApiConfiguration ?? new FiksApiConfiguration();
            CatalogConfiguration = catalogConfiguration ?? new CatalogConfiguration(FiksApiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                                            null,
                                            FiksApiConfiguration.Scheme,
                                            FiksApiConfiguration.Host,
                                            FiksApiConfiguration.Port);
        }

        public AccountConfiguration AccountConfiguration { get; }

        public CatalogConfiguration CatalogConfiguration { get; }

        public FiksApiConfiguration FiksApiConfiguration { get; }

        public FiksIntegrationConfiguration FiksIntegrationConfiguration { get; }

        public FiksIOSenderConfiguration FiksIOSenderConfiguration { get; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; }
    }
}