using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class FiksIntegrationConfiguration
    {
        public FiksIntegrationConfiguration(Guid integrastionId, string integrationPassword)
        {
            IntegrastionId = integrastionId;
            IntegrationPassword = integrationPassword;
        }

        public Guid IntegrastionId { get; }

        public string IntegrationPassword { get; }

        public string Scope => "ks";
    }
}