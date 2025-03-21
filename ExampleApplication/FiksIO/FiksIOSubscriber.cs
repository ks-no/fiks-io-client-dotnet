using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.ASiC_E;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ExampleApplication.FiksIO
{
    public class FiksIOSubscriber : BackgroundService
    {
        private readonly IFiksIOClient _fiksIoClient;
        private readonly AppSettings _appSettings;
        private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);

        public FiksIOSubscriber(IFiksIOClient fiksIoClient, AppSettings appSettings)
        {
            _fiksIoClient = fiksIoClient;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("FiksIOSubscriber - Application is starting subscribe");
            
            await SubscribeToFiksIOClient();
        }

        private async Task OnReceivedMelding(MottattMeldingArgs mottatt)
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
                    var payloadTxt = await GetDecryptedPayloadTxt(mottatt);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksArkivPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksArkivPong, klientMeldingId);
                    var payloadTxt = await GetDecryptedPayloadTxt(mottatt);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksPlanPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksPlanPong, klientMeldingId);
                    var payloadTxt = await GetDecryptedPayloadTxt(mottatt);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}", sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
                case Program.FiksMatrikkelfoeringPing:
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = await mottatt.SvarSender.Svar(Program.FiksMatrikkelfoeringPong, klientMeldingId);
                    var payloadTxt = await GetDecryptedPayloadTxt(mottatt);
                    Log.Information("FiksIOSubscriber - Received {receivedMeldingType} with payload text {payload}. Replied messagetype 'ping' with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}",sendtMelding.MeldingType, payloadTxt, sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
            }

            await mottatt.SvarSender.AckAsync(); 
        }

        private static async Task<string> GetDecryptedPayloadTxt(MottattMeldingArgs mottattMeldingArgs)
        {
            var payloadTxt = "empty";
            
            IAsicReader asiceReader = new AsiceReader();
            using var asiceReadModel = asiceReader.Read(await mottattMeldingArgs.Melding.DecryptedStream);

              
            // Verify asice and read payload
            foreach (var asiceVerifyReadEntry in asiceReadModel.Entries)
            {
                await using (var entryStream = asiceVerifyReadEntry.OpenStream())
                {
                    Log.Information($"GetDecryptedPayloadTxt - {asiceVerifyReadEntry.FileName}");
                    await using var fileStream = new FileStream($"received-{asiceVerifyReadEntry.FileName}", FileMode.Create, FileAccess.Write);
                    await entryStream.CopyToAsync(fileStream);
                }
                payloadTxt = await File.ReadAllTextAsync($"received-{asiceVerifyReadEntry.FileName}");
            }            
            
            // Only one payload in this example, and only one text
            return payloadTxt;
        }

     private async Task SubscribeToFiksIOClient()
        {
            var accountId = _appSettings.FiksIOConfig.FiksIoAccountId;
            Log.Information($"FiksIOSubscriber - Starting FiksIOReceiveAndReplySubscriber subscribe on account {accountId}...");
            await _fiksIoClient.NewSubscriptionAsync(OnReceivedMelding);
        }
    }
}