using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ExampleApplication.FiksIO;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Amqp.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace ExampleApplication
{
    /**
     * Shows how to integrate with Fiks-IO using KS Fiks-IO-Client-dotnet.
     * This program subscribes to incoming Fiks-IO messages and replies to 'ping' messages with a 'pong' message.
     * It also sends the 'ping' message to it's own Fiks-IO account when pressing the Enter-key, or other keys for Fiks-Protokoll accounts.
     * If you're using a Fiks-Protokoll account, you will have to use the appropriate 'ping' or 'pong' message for that protocol.
     * See available constants given in the code and instructions in the console when starting the application.
     * Prerequisites:
     *  - A Fiks-IO account or a Fiks-Protokoll account - see documentation on how to to this at https://developers.fiks.ks.no/
     *    
     */
    class Program
    {
        private static MessageSender _messageSender;
        private static Guid _toAccountId;
        private static ILogger _logger;
        public const string FiksIOPing = "ping";
        public const string FiksIOPong = "pong";
        public const string FiksArkivPing = "no.ks.fiks.arkiv.v1.ping";
        public const string FiksArkivPong = "no.ks.fiks.arkiv.v1.pong";
        public const string FiksPlanPing = "no.ks.fiks.plan.v2.ping";
        public const string FiksPlanPong = "no.ks.fiks.plan.v2.pong";
        public const string FiksMatrikkelfoeringPing = "no.ks.fiks.matrikkelfoering.v2.ping";
        public const string FiksMatrikkelfoeringPong = "no.ks.fiks.matrikkelfoering.v2.pong";
        private static RabbitMQEventLogger _rabbitMqEventLogger;
        
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
            _rabbitMqEventLogger = new RabbitMQEventLogger(loggerFactory, EventLevel.Informational);
            
            // Creating messageSender as a local instance
            _messageSender = new MessageSender(fiksIoClient, appSettings);
            
            // Setting the account to send messages to. In this case the same as sending account
            _toAccountId = appSettings.FiksIOConfig.FiksIoAccountId;
            
            _logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

            var consoleKeyTask = Task.Run(() => { MonitorKeypress(); });

            await new HostBuilder()
                .ConfigureHostConfiguration((configHost) =>
                {
                    configHost.AddEnvironmentVariables("DOTNET_");
                })
                .ConfigureServices((_, services) =>
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
            _logger.Information("Press Enter-key for sending a Fiks-IO 'ping' message");
            _logger.Information("Press A-key for sending a Fiks-Arkiv V1 'ping' message");
            _logger.Information("Press P-key for sending a Fiks-Plan V2 'ping' message");
            _logger.Information("Press M-key for sending a Fiks-Matrikkelfoering V2 'ping' message");


            ConsoleKeyInfo cki;
            do 
            {
                //var cki = new ConsoleKeyInfo();
                // true hides the pressed character from the console
                cki = Console.ReadKey(true);

                var key = cki.Key;

                if (key == ConsoleKey.Enter)
                {
                    _logger.Information("Enter pressed. Sending Fiks-IO ping-message to account id: {ToAccountId}", _toAccountId);
                    await _messageSender.Send(FiksIOPing, _toAccountId);
                } else if (key == ConsoleKey.A)
                {
                    _logger.Information("A-key pressed. Sending Fiks-Arkiv V1 ping-message to account id: {ToAccountId}", _toAccountId);
                    await _messageSender.Send(FiksArkivPing, _toAccountId);
                } else if (key == ConsoleKey.P)
                {
                    _logger.Information("P-key pressed. Sending Fiks-Plan V2 ping-message to account id: {ToAccountId}", _toAccountId);
                    await _messageSender.Send(FiksPlanPing, _toAccountId);
                } else if (key == ConsoleKey.M)
                {
                    _logger.Information("M-key pressed. Sending Fiks-Matrikkelfoering V2 ping-message to account id: {ToAccountId}", _toAccountId);
                    await _messageSender.Send(FiksMatrikkelfoeringPing, _toAccountId);
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