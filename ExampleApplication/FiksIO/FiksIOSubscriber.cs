using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ExampleApplication.FiksIO
{
    public class FiksIOSubscriber : BackgroundService
    {
        private IFiksIOClient _fiksIoClient;
        private readonly AppSettings _appSettings;
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
        private Timer FiksIoClientStatusCheckTimer { get; set; }
        private const int HealthCheckInterval = 15 * 1000;

        public FiksIOSubscriber(IFiksIOClient fiksIoClient, AppSettings appSettings)
        {
            _fiksIoClient = fiksIoClient;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("FiksIOSubscriber - Application is starting subscribe");
            FiksIoClientStatusCheckTimer = new Timer(WriteStatusToLog, null, HealthCheckInterval, HealthCheckInterval);
            SubscribeToFiksIOClient();
            await Task.CompletedTask;
        }
    
        private async void OnReceivedMelding(object sender, MottattMeldingArgs mottatt)
        {
            var receivedMeldingType = mottatt.Melding.MeldingType;
            var konto = await _fiksIoClient.GetKonto(mottatt.Melding.AvsenderKontoId);
            
            Log.Information("FiksIOSubscriber - Received a message with messagetype '{MessageType}' with following attributes: " +
                            "\n\t messageId : {MeldingId}" +
                            "\n\t klientMeldingId : {KlientMeldingId}" +
                            "\n\t svarPaMelding : {SvarPaMelding}" + 
                            "\n\t fiksOrgNavn : {FiksOrgNavn}" , 
                receivedMeldingType, mottatt.Melding.MeldingId, mottatt.Melding.KlientMeldingId, mottatt.Melding.SvarPaMelding, konto.FiksOrgNavn);
            
            switch (receivedMeldingType)
            {
                case Program.FiksIOPong:
                case Program.FiksArkivPong:
                case Program.FiksPlanPong:
                case Program.FiksMatrikkelfoeringPong:
                    Log.Information($"FiksIOSubscriber - Received {receivedMeldingType}. Do nothing with 'pong' message. End of correspondence for now.");
                    break;
                case Program.FiksIOPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksIOPong, klientMeldingId);
                    var decryptedMessagePayloads = await mottatt.Melding.DecryptedPayloads;
                    var payloadTxt = await GetDecryptedPayloadTxt(decryptedMessagePayloads);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksArkivPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksArkivPong, klientMeldingId);
                    var decryptedMessagePayloads = await mottatt.Melding.DecryptedPayloads;
                    var payloadTxt = await GetDecryptedPayloadTxt(decryptedMessagePayloads);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksPlanPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksPlanPong, klientMeldingId);
                    var decryptedMessagePayloads = await mottatt.Melding.DecryptedPayloads;
                    var payloadTxt = await GetDecryptedPayloadTxt(decryptedMessagePayloads);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksMatrikkelfoeringPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksMatrikkelfoeringPong, klientMeldingId);
                    var decryptedMessagePayloads = await mottatt.Melding.DecryptedPayloads;
                    var payloadTxt = await GetDecryptedPayloadTxt(decryptedMessagePayloads);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}",sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
            }

            mottatt.SvarSender.Ack(); 
        }

        private static async Task<string> GetDecryptedPayloadTxt(IEnumerable<IPayload> decryptedMessagePayloads)
        {
            var payloadTxt = "empty";
            foreach (var payload in decryptedMessagePayloads)
            {
                
            }

            return payloadTxt;
        }

        private void SubscribeToFiksIOClient()
        {
            var accountId = _appSettings.FiksIOConfig.FiksIoAccountId;
            Log.Information($"FiksIOSubscriber - Starting FiksIOReceiveAndReplySubscriber subscribe on account {accountId}...");
            _fiksIoClient.NewSubscription(OnReceivedMelding);
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
    }
}