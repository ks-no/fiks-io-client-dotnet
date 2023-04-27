using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KS.Fiks.IO.Client
{
    public static class FiksIOClientServiceProvider
    {
        public static IServiceCollection AddServiceForFiksIOClient(this IServiceCollection provider, FiksIOConfiguration fiksIoConfiguration)
        {
            provider.AddSingleton(fiksIoConfiguration);
            provider.AddScoped<IFiksIOClient, FiksIOClient>();
            return provider;
        }
    }
}