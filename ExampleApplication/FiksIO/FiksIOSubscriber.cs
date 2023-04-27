using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace ExampleApplication
{
    public class FiksIOSubscriber : BackgroundService
    {
        private IFiksIOClient _fiksIoClient;
        private readonly AppSettings _appSettings;
        private readonly ILoggerFactory _loggerFactory;
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private Timer _FiksIOClientStatusCheckTimer { get; set; }
        private readonly IServiceScopeFactory _scopeFactory;
        private const int HealthCheckInterval = 15 * 1000;

        public FiksIOSubscriber(IFiksIOClient fiksIoClient, AppSettings appSettings)
        {
            _fiksIoClient = fiksIoClient;
            _appSettings = appSettings;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // await FiksIOClient initialization
            await _fiksIoClient.GetInitialization();
            
            Log.Information("Application is starting subscribe");
            SubscribeToFiksIOClient();
            
            Log.Information($"FiksIOSubscriber is starting timer for simple health checks with interval of {HealthCheckInterval} ms");

            _FiksIOClientStatusCheckTimer = new Timer(WriteStatusToLog, null, HealthCheckInterval, HealthCheckInterval);

            await Task.CompletedTask;
        }
        
        private void WriteStatusToLog(object o)
        {
            Log.Information($"FiksIOSubscriber status check - FiksIOClient connection IsOpen status: {_fiksIoClient.IsOpen()}");
            Log.Information($"FiksIOSubscriber status check - Maskinporten reachable : {CheckMaskinportenIsReachable()}");
        }

        private bool CheckMaskinportenIsReachable()
        {
            var request = (HttpWebRequest) WebRequest.Create(_appSettings.FiksIOConfig.MaskinPortenAudienceUrl);
            request.Timeout = 5 * 1000;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private void OnReceivedMelding(object sender, MottattMeldingArgs mottatt)
        {
            Log.Information("Message with messageId {MeldingId} and messagetype {MeldingsType} received. Message will be acked.", mottatt.Melding.MeldingId,
                mottatt.Melding.MeldingType);
            mottatt.SvarSender.Ack(); 
        }

        private void SubscribeToFiksIOClient()
        {
            var accountId = _appSettings.FiksIOConfig.FiksIoAccountId;
            Log.Information($"Starting subscribe on account {accountId}...");
            _fiksIoClient.NewSubscription(OnReceivedMelding);
        }
    }
}