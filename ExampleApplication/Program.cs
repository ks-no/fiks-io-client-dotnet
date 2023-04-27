using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ExampleApplication.FiksIO;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Configuration;
using KS.Fiks.IO.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ExampleApplication
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var loggerFactory = InitSerilogConfiguration();
            await new HostBuilder()
                .ConfigureHostConfiguration((configHost) =>
                {
                    configHost.AddEnvironmentVariables("DOTNET_");
                })
                .ConfigureAppConfiguration((hostBuilder, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddJsonFile($"appsettings.Development.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var appSettings = AppSettingsBuilder.CreateAppSettings(hostContext.Configuration);
                    services.AddSingleton(appSettings);
                    services.AddSingleton(loggerFactory);
                    services.AddServiceForFiksIOClient(FiksIoConfigurationBuilder.CreateConfiguration(appSettings));
                    services.AddHostedService<FiksIOSubscriber>();
                })
                .RunConsoleAsync();
        }
        
        private static ILoggerFactory InitSerilogConfiguration()
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

            var logger = loggerConfiguration.CreateLogger();
            Log.Logger = logger;

            return LoggerFactory.Create(logging => logging.AddSerilog(logger));
        }
    }
}