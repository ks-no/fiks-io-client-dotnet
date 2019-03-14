using System;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIOConfiguration
    {
        public AccountConfiguration AccountConfiguration { get; set; }

        public CatalogConfiguration CatalogConfiguration { get; set; }

        public MaskinportenClientConfiguration MaskinportenConfiguration { get; set; }

        public string IntegrasjonPassword { get; set; }

        public Guid IntegrasjonId { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Scheme { get; set; }

        public string Path { get; set; }
    }
}