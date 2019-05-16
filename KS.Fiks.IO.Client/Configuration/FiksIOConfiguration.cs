using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public FiksIOConfiguration(
            AccountConfiguration accountConfiguration,
            IntegrationConfiguration integrationConfiguration,
            MaskinportenClientConfiguration maskinportenConfiguration,
            ApiConfiguration apiConfiguration = null,
            AmqpConfiguration amqpConfiguration = null,
            CatalogConfiguration catalogConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null,
            DokumentlagerConfiguration dokumentlagerConfiguration = null)
        {
            AccountConfiguration = accountConfiguration;
            IntegrationConfiguration = integrationConfiguration;
            MaskinportenConfiguration = maskinportenConfiguration;
            ApiConfiguration = apiConfiguration ?? new ApiConfiguration();
            AmqpConfiguration = amqpConfiguration ?? new AmqpConfiguration(ApiConfiguration.Host);
            CatalogConfiguration = catalogConfiguration ?? new CatalogConfiguration(ApiConfiguration);
            DokumentlagerConfiguration = dokumentlagerConfiguration ?? new DokumentlagerConfiguration(apiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                                            null,
                                            ApiConfiguration.Scheme,
                                            ApiConfiguration.Host,
                                            ApiConfiguration.Port);
        }

        public AccountConfiguration AccountConfiguration { get; }

        public AmqpConfiguration AmqpConfiguration { get; }

        public CatalogConfiguration CatalogConfiguration { get; }

        public ApiConfiguration ApiConfiguration { get; }

        public IntegrationConfiguration IntegrationConfiguration { get; }

        public FiksIOSenderConfiguration FiksIOSenderConfiguration { get; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; }

        public DokumentlagerConfiguration DokumentlagerConfiguration { get; }
    }
}