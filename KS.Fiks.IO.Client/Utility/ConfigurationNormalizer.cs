using KS.Fiks.IO.Client.Configuration;

namespace KS.Fiks.IO.Client.Utility
{
    public static class ConfigurationNormalizer
    {
        public static FiksIOConfiguration SetDefaultValues(FiksIOConfiguration configuration)
        {
            configuration.CatalogConfiguration =
                configuration.CatalogConfiguration ?? GetDefaultCatalogConfiguration(configuration); 
            return configuration;
        }

        private static CatalogConfiguration GetDefaultCatalogConfiguration(FiksIOConfiguration configuration)
        {
            return new CatalogConfiguration
            {
                Host = configuration.Host,
                Path = configuration.Path,
                Port = configuration.Port,
                Scheme = configuration.Scheme
            };
        }
    }
}