using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIntegrationConfiguration
    {
        public FiksIntegrationConfiguration(Guid integrationId, string integrationPassword)
        {
            IntegrationId = integrationId;
            IntegrationPassword = integrationPassword;
        }

        public Guid IntegrationId { get; }

        public string IntegrationPassword { get; }

        public string Scope => "ks";
    }
}