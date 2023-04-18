using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace KS.Fiks.IO.Client
{
    public static class FiksIOClientServiceProvider
    {
        public static IServiceCollection AddServiceForFiksIOClient(this IServiceCollection provider)
        {
            provider.AddScoped<IFiksIOClient, FiksIOClient>();
            return provider;
        }
    }
}