using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ExampleApplication.FiksIO;
using KS.Fiks.IO.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ExampleApplication
{
    /**
     * Shows how to integrate with Fiks-IO using KS Fiks-IO-Client-dotnet.
     * This program subscribes to incoming Fiks-IO messages and replies to 'ping' messages with a 'pong' message.
     * It also sends the 'ping' message to it's own Fiks-IO account when pressing the Enter-key.
     * If you're using a Fiks-Protokoll account, you will have to use the appropriate 'ping' or 'pong' message for that protocol.
     * See available constants given in the code.
     * Prerequisites:
     *  - A Fiks-IO account og a Fiks-Protokoll account - see documentation on how to to this at https://developers.fiks.ks.no/
     *    
     */
    class Program
    {
        private static MessageSender messageSender;
        private static Guid toAccountId;
        private static Serilog.ILogger Logger;
        public const string FiksIOPing = "ping";
        public const string FiksIOPong = "pong";
        public const string FiksArkivPing = "no.ks.fiks.arkiv.v1.ping";
        public const string FiksArkivPong = "no.ks.fiks.arkiv.v1.pong";
        
        public static async Task Main(string[] args)
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true).Build();
            
            var loggerFactory = InitSerilogConfiguration();
            var appSettings = AppSettingsBuilder.CreateAppSettings(configurationRoot);
            var configuration = FiksIoConfigurationBuilder.CreateConfiguration(appSettings);
            var fiksIoClient = await FiksIOClient.CreateAsync(configuration, loggerFactory);
            
            // Creating messageSender as a local instance
            messageSender = new MessageSender(fiksIoClient, appSettings);
            
            // Setting the account to send messages to. In this case the same as sending account
            toAccountId = appSettings.FiksIOConfig.FiksIoAccountId;
            
            Logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
            
            var consoleKeyTask = Task.Run(() => { MonitorKeypress(); });

            await new HostBuilder()
                .ConfigureHostConfiguration((configHost) =>
                {
                    configHost.AddEnvironmentVariables("DOTNET_");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(appSettings);
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton<IFiksIOClient>(fiksIoClient);
                    services.AddHostedService<FiksIOSubscriber>();
                })
                .RunConsoleAsync();
            
            await consoleKeyTask;
        }

        private static async Task MonitorKeypress()
        {
            Logger.Information("Press Enter-key for sending a Fiks-IO 'ping' message");
            Logger.Information("Press A-key for sending a Fiks-Arkiv 'ping' message");
            var cki = new ConsoleKeyInfo();
            do 
            {
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);
                var key = cki.Key;

                if (key == ConsoleKey.Enter)
                {
                    Logger.Information("Enter pressed. Sending Fiks-IO ping-message to account id: {ToAccountId}", toAccountId);
                    var sendtMessageId = await messageSender.Send(FiksIOPing, toAccountId);
                } else if (key == ConsoleKey.A)
                {
                    Logger.Information("A-key pressed. Sending Fiks-Arkiv ping-message to account id: {ToAccountId}", toAccountId);
                    var sendtMessageId = await messageSender.Send(FiksArkivPing, toAccountId);
                }
    
                // Wait for an ESC
            } while (cki.Key != ConsoleKey.Escape);
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