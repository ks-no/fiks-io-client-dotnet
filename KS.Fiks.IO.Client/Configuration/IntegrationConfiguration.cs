using System;

namespace KS.Fiks.IO.Client.Configuration
{
    public class IntegrationConfiguration
    {
        private const string DefaultScope = "ks";

        public IntegrationConfiguration(Guid integrationId, string integrationPassword, string scope = null)
        {
            IntegrationId = integrationId;
            IntegrationPassword = integrationPassword;
            Scope = scope ?? DefaultScope;
        }

        public Guid IntegrationId { get; }

        public string IntegrationPassword { get; }

        public string Scope { get; }
    }
}