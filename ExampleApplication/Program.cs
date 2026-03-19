using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExampleApplication.FiksIO;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Amqp.RabbitMQ;
using Ks.Fiks.Maskinporten.Client;
using Ks.Fiks.Protokoll;
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
        private static FiksIOClient _fiksIoClient;
        private static FiksProtokollKonfigurasjonApiClient _fiksProtokollKonfigurasjonApiClient;
        private static AppSettings appSettings;
        private static Guid _toAccountId;
        private static ILogger _logger;
        private static MaskinportenClient _maskinportenClient;
        private static string _scope;
        public const string FiksIOPing = "ping";
        public const string FiksIOPong = "pong";
        public const string FiksArkivPing = "no.ks.fiks.arkiv.v1.ping";
        public const string FiksArkivPong = "no.ks.fiks.arkiv.v1.pong";
        public const string FiksPlanPing = "no.ks.fiks.plan.v2.ping";
        public const string FiksPlanPong = "no.ks.fiks.plan.v2.pong";
        public const string FiksMatrikkelfoeringPing = "no.ks.fiks.matrikkelfoering.v2.ping";
        public const string FiksMatrikkelfoeringPong = "no.ks.fiks.matrikkelfoering.v2.pong";
        private static RabbitMQEventLogger _rabbitMqEventLogger;
        
        private static Guid _protokollFiksOrgId = Guid.Empty;
        private static Guid _protokollSystemId = Guid.Empty;
        private static Guid _protokollKontoId = Guid.Empty;
        
        public static async Task Main(string[] args)
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Test.json", optional: true).Build();

            var loggerFactory = InitSerilogConfiguration();
            appSettings = AppSettingsBuilder.CreateAppSettings(configurationRoot);
            var configuration = FiksIoConfigurationBuilder.CreateTestConfiguration(appSettings);
            
            _maskinportenClient = new MaskinportenClient(configuration.MaskinportenConfiguration);
            _scope = configuration.IntegrasjonConfiguration.Scope;
            
            _fiksIoClient = await FiksIOClient.CreateAsync(configuration, loggerFactory);
            _rabbitMqEventLogger = new RabbitMQEventLogger(loggerFactory, EventLevel.Informational);
            
            // Creating messageSender as a local instance
            _messageSender = new MessageSender(_fiksIoClient, appSettings);
            
            _protokollSystemId = appSettings.FiksIOConfig.ProtokollSystemId;
            
            var protokollPublicKey = appSettings.FiksIOConfig.ProtokollPublicKey;
            Log.Information("Initialized Protokoll System ID: {SystemId}", _protokollSystemId);
            Log.Information("Protokoll Public Key path: {PublicKeyPath}", protokollPublicKey);
            
            _fiksProtokollKonfigurasjonApiClient = await InitializeFiksProtokollKonfigurasjonApiClient(appSettings);
            
            // Setting the account to send messages to. In this case the same as sending account
            _toAccountId = appSettings.FiksIOConfig.FiksIoAccountId;
            _protokollFiksOrgId = _fiksIoClient.GetKonto(_toAccountId).Result.FiksOrgId;

            _logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

            var tokenSource = new CancellationTokenSource();

            Task.Run(() => { MonitorKeypress(tokenSource); });
            
            await new HostBuilder()
                .ConfigureHostConfiguration(configHost => { configHost.AddEnvironmentVariables("DOTNET_"); })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton(appSettings);
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton<IFiksIOClient>(_fiksIoClient);
                    services.AddHostedService<FiksIOSubscriber>();
                })
                .RunConsoleAsync(tokenSource.Token);
        }

        private static async Task MonitorKeypress(CancellationTokenSource tokenSource)
        {
            _logger.Information("Press Enter-key for sending a Fiks-IO 'ping' message");
            _logger.Information("Press A-key for sending a Fiks-Arkiv V1 'ping' message");
            _logger.Information("Press P-key for sending a Fiks-Plan V2 'ping' message");
            _logger.Information("Press M-key for sending a Fiks-Matrikkelfoering V2 'ping' message");
            _logger.Information("Press L-key for printing status information in console");
            _logger.Information("Press T-key for generating a Maskinporten token");
            _logger.Information("Press N-key for creating a Fiks Arkiv konto");
            _logger.Information("Press C-key for sending access request to created konto");
            _logger.Information("Press V-key for viewing all access requests");
            _logger.Information("Press R-key for approving all access requests");
            _logger.Information("Press Q-key to exit");

            ConsoleKeyInfo cki;
            do 
            {
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
                } else if (key == ConsoleKey.L)
                {
                    await WriteHeartBeatConnectionStatusToLog();
                    await WriteStatusFromApiToLog();
                } else if (key == ConsoleKey.T)
                {
                    _logger.Information("T-key pressed. Generating a Maskinporten token");
                    await WriteMaskinportenToken();
                } else if (key == ConsoleKey.N)
                {
                    _logger.Information("N-key pressed. Creating Fiks Arkiv konto");
                    await CreateFiksArkivKontoAsync();
                } else if (key == ConsoleKey.C)
                {
                    _logger.Information("C-key pressed. Sending access request to konto");
                    await SendAccessRequestAsync();
                } else if (key == ConsoleKey.V)
                {
                    _logger.Information("V-key pressed. Viewing all access requests");
                    await ViewAccessRequestsAsync();
                } else if (key == ConsoleKey.R)
                {
                    _logger.Information("R-key pressed. Approving all access requests");
                    await ApproveAccessRequestsAsync();
                }
            } while (cki.Key != ConsoleKey.Q);
            
            _logger.Information("Q-key pressed. Closing application");
            await tokenSource.CancelAsync();
        }

        private static async Task WriteMaskinportenToken()
        {
            MaskinportenToken token = await _maskinportenClient.GetAccessToken(_scope);
            Log.Information("New Maskinporten token, expires in {ExpirationTime} seconds", 120);
            Log.Information(token.Token);
        }
        
        private static async Task CreateFiksArkivKontoAsync()
        {
            try
            {
                Log.Information("Creating Fiks Arkiv konto...");
                
                if (_protokollSystemId == Guid.Empty)
                {
                    Log.Warning("SystemId not set. Please configure in appsetings.json");
                    return;
                }
                
                var arkivPart = new PartRequest
                {
                    PartNavn = "arkiv.full",
                    StottetProtokollVersjon = "v1"
                };

                var parts = new List<PartRequest> {arkivPart };
                
                string offentligNokkel = "";
                var publicKeyPath = appSettings.FiksIOConfig.ProtokollPublicKey;
                if (!string.IsNullOrEmpty(publicKeyPath) && File.Exists(publicKeyPath))
                {
                    offentligNokkel = File.ReadAllText(publicKeyPath);
                    Log.Information("Loaded public key from: {PublicKeyPath}", publicKeyPath);
                }
                else
                {
                    Log.Warning("Public key file not found at: {PublicKeyPath}", publicKeyPath);
                }
                
                var createKontoRequest = new CreateProtokollKontoRequest
                {
                    Navn = "Fiks Arkiv Konto",
                    Beskrivelse = "Konto for å sende meldinger til Arkiv",
                    StottetProtokollNavn = "no.ks.fiks.arkiv",
                    Parts = parts,
                    OffentligNokkel = !string.IsNullOrEmpty(offentligNokkel) ? offentligNokkel : null
                };

                var response = await _fiksProtokollKonfigurasjonApiClient.CreateKontoAsync(_protokollFiksOrgId, _protokollSystemId, createKontoRequest);
                
                if (response.StatusCode >= 200 && response.StatusCode < 300 && response.Result != null)
                {
                    _protokollKontoId = response.Result.Id;
                    Log.Information("Successfully created Fiks Arkiv konto with ID: {KontoId}", _protokollKontoId);
                    Log.Information("Konto navn: {KontoNavn}", response.Result.Navn);
                    Log.Information("Protokoll: {Protokoll}", response.Result.StottetProtokollNavn);
                }
                else
                {
                    Log.Error("Failed to create Fiks Arkiv konto (Status: {StatusCode})", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating Fiks Arkiv konto");
            }
        }

        private static async Task SendAccessRequestAsync()
        {
            try
            {
                if (_protokollKontoId == Guid.Empty)
                {
                    Log.Warning("No konto created yet. Please create a konto first (press N)");
                    return;
                }

                if (_protokollFiksOrgId == Guid.Empty || _protokollSystemId == Guid.Empty)
                {
                    Log.Warning("FiksOrgId or SystemId not set");
                    return;
                }

                Log.Information("Sending access request to konto {KontoId}...", _protokollKontoId);
                
                var response = await _fiksProtokollKonfigurasjonApiClient.CreateTilgangForesporselTilKontoAsync(
                    _protokollFiksOrgId, 
                    _protokollSystemId, 
                    _protokollKontoId);
                
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    Log.Information("Successfully sent access request to konto {KontoId}", _protokollKontoId);
                }
                else
                {
                    Log.Error("Failed to send access request (Status: {StatusCode})", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error sending access request");
            }
        }

        private static async Task ViewAccessRequestsAsync()
        {
            try
            {
                if (_protokollKontoId == Guid.Empty)
                {
                    Log.Warning("No konto created yet. Please create a konto first (press N)");
                    return;
                }

                if (_protokollFiksOrgId == Guid.Empty || _protokollSystemId == Guid.Empty)
                {
                    Log.Warning("FiksOrgId or SystemId not set");
                    return;
                }

                Log.Information("Fetching access requests for konto {KontoId}...", _protokollKontoId);
                
                var response = await _fiksProtokollKonfigurasjonApiClient.GetForespurteTilgangerPaaKontoAsync(
                    _protokollFiksOrgId,
                    _protokollSystemId,
                    _protokollKontoId);
                
                if (response.StatusCode >= 200 && response.StatusCode < 300 && response.Result != null)
                {
                    var accessRequests = response.Result.ToList();
                    Log.Information("Found {Count} access requests", accessRequests.Count);
                    
                    foreach (var request in accessRequests)
                    {
                        Log.Information("  - System: {SystemNavn}, Org: {OrgNavn}", request.Navn, request.FiksOrgNavn);
                    }
                }
                else
                {
                    Log.Error("Failed to fetch access requests (Status: {StatusCode})", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error viewing access requests");
            }
        }

        private static async Task ApproveAccessRequestsAsync()
        {
            try
            {
                if (_protokollKontoId == Guid.Empty)
                {
                    Log.Warning("No konto created yet. Please create a konto first (press N)");
                    return;
                }

                if (_protokollFiksOrgId == Guid.Empty || _protokollSystemId == Guid.Empty)
                {
                    Log.Warning("FiksOrgId or SystemId not set");
                    return;
                }

                Log.Information("Approving access requests for konto {KontoId}...", _protokollKontoId);
                
                var response = await _fiksProtokollKonfigurasjonApiClient.GetForespurteTilgangerPaaKontoAsync(
                    _protokollFiksOrgId,
                    _protokollSystemId,
                    _protokollKontoId);
                
                if (response.StatusCode >= 200 && response.StatusCode < 300 && response.Result != null)
                {
                    int approvedCount = 0;
                    foreach (var accessRequest in response.Result)
                    {
                        var approveResponse = await _fiksProtokollKonfigurasjonApiClient.CreateTilgangTilSystemAsync(
                            _protokollFiksOrgId,
                            _protokollSystemId,
                            _protokollKontoId,
                            accessRequest.Id);
                        
                        if (approveResponse.StatusCode >= 200 && approveResponse.StatusCode < 300)
                        {
                            approvedCount++;
                            Log.Information("Approved access for system: {SystemNavn}", accessRequest.Navn);
                        }
                        else
                        {
                            Log.Error("Failed to approve access for system {SystemId} (Status: {StatusCode})", accessRequest.Id, approveResponse.StatusCode);
                        }
                    }
                    
                    Log.Information("Approved {Count} access requests", approvedCount);
                }
                else
                {
                    Log.Error("Failed to fetch access requests for approval (Status: {StatusCode})", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error approving access requests");
            }
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
        
        private static async Task WriteHeartBeatConnectionStatusToLog()
        {
            var isOpen = await _fiksIoClient.IsOpenAsync();
            Log.Information($"FiksIOSubscriber status check - FiksIOClient connection IsOpen: {isOpen}");
        }

        private static async Task WriteStatusFromApiToLog()
        {
            var konto = await _fiksIoClient.GetKonto(appSettings.FiksIOConfig.FiksIoAccountId).ConfigureAwait(false);
            var status = await _fiksIoClient.GetKontoStatus(appSettings.FiksIOConfig.FiksIoAccountId).ConfigureAwait(false);
            var keyValidation = await _fiksIoClient.ValidatePublicKeyAgainstPrivateKeyAsync().ConfigureAwait(false);
            Log.Information($"FiksIOSubscriber status check - FiksIOClient Konto - antallkonsumenter  : {konto.AntallKonsumenter}");
            Log.Information($"FiksIOSubscriber status check - FiksIOClient KontoStatus - antallkonsumenter : {status.AntallKonsumenter}");
            Log.Information($"FiksIOSubscriber status check - FiksIOClient KontoStatus - antall uavhentede meldinger : {status.AntallUavhentedeMeldinger}");
            Log.Information($"FiksIOSubscriber status check - FiksIOClient Key Validation - does the public key from catalog match the private key configured in client: {keyValidation}");
        }

        private static async Task<FiksProtokollKonfigurasjonApiClient> InitializeFiksProtokollKonfigurasjonApiClient(AppSettings appSettingsParam)
        {
            var integrasjonId = appSettingsParam.FiksIOConfig.FiksIoIntegrationId;
            var integrasjonPassword = appSettingsParam.FiksIOConfig.FiksIoIntegrationPassword;
            var apiScheme = appSettingsParam.FiksIOConfig.ApiScheme;
            var apiHost = appSettingsParam.FiksIOConfig.ApiHost;
            var apiPort = appSettingsParam.FiksIOConfig.ApiPort;
            var baseUrl = $"{apiScheme}://{apiHost}:{apiPort}";
            
            var handler = new MaskinportenTokenDelegatingHandler(_maskinportenClient, _scope, integrasjonId, integrasjonPassword)
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(baseUrl);

            return new FiksProtokollKonfigurasjonApiClient(httpClient)
            {
                BaseUrl = baseUrl
            };
        }

        private class MaskinportenTokenDelegatingHandler(
            MaskinportenClient maskinportenClient,
            string scope,
            Guid integrasjonId,
            string integrasjonPassword)
            : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                request.Headers.Add("IntegrasjonId", integrasjonId.ToString());
                request.Headers.Add("IntegrasjonPassord", integrasjonPassword);
                
                var token = await maskinportenClient.GetAccessToken(scope);
                
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
                
                request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());

                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}