using KS.Fiks.IO.Send.Client.Configuration;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public FiksIOConfiguration(
            KontoConfiguration kontoConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            MaskinportenClientConfiguration maskinportenConfiguration,
            ApiConfiguration apiConfiguration = null,
            AmqpConfiguration amqpConfiguration = null,
            KatalogConfiguration katalogConfiguration = null,
            FiksIOSenderConfiguration fiksIOSenderConfiguration = null,
            DokumentlagerConfiguration dokumentlagerConfiguration = null)
        {
            KontoConfiguration = kontoConfiguration;
            IntegrasjonConfiguration = integrasjonConfiguration;
            MaskinportenConfiguration = maskinportenConfiguration;
            ApiConfiguration = apiConfiguration ?? new ApiConfiguration();
            AmqpConfiguration = amqpConfiguration ?? new AmqpConfiguration(ApiConfiguration.Host);
            KatalogConfiguration = katalogConfiguration ?? new KatalogConfiguration(ApiConfiguration);
            DokumentlagerConfiguration = dokumentlagerConfiguration ?? new DokumentlagerConfiguration(ApiConfiguration);
            FiksIOSenderConfiguration = fiksIOSenderConfiguration ?? new FiksIOSenderConfiguration(
                                            null,
                                            ApiConfiguration.Scheme,
                                            ApiConfiguration.Host,
                                            ApiConfiguration.Port);
        }

        public KontoConfiguration KontoConfiguration { get; }

        public AmqpConfiguration AmqpConfiguration { get; }

        public KatalogConfiguration KatalogConfiguration { get; }

        public ApiConfiguration ApiConfiguration { get; }

        public IntegrasjonConfiguration IntegrasjonConfiguration { get; }

        public FiksIOSenderConfiguration FiksIOSenderConfiguration { get; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; }

        public DokumentlagerConfiguration DokumentlagerConfiguration { get; }
    }
}