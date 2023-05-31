﻿using System;
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
            Log.Information("Application is starting subscribe");
            FiksIoClientStatusCheckTimer = new Timer(WriteStatusToLog, null, HealthCheckInterval, HealthCheckInterval);
            SubscribeToFiksIOClient();
            await Task.CompletedTask;
        }
    
        private void OnReceivedMelding(object sender, MottattMeldingArgs mottatt)
        {
            var receivedMeldingType = mottatt.Melding.MeldingType;

            switch (receivedMeldingType)
            {
                case "pong":
                    Log.Information("Received the reply message with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId : {KlientMeldingId}!", mottatt.Melding.MeldingId, mottatt.Melding.KlientMeldingId);
                    break;
                case "ping":
                {
                    var klientMeldingId = Guid.NewGuid();
                    var sendtMelding = mottatt.SvarSender.Svar("pong", klientMeldingId).Result;
                    Log.Information("Received message with messagetype 'ping' and replied message with messagetype 'pong' with messageId : {MeldingId} and klientMeldingId: {KlientMeldingId}!", sendtMelding.MeldingId, sendtMelding.KlientMeldingId);
                    break;
                }
            }

            mottatt.SvarSender.Ack(); 
        }

        private void SubscribeToFiksIOClient()
        {
            var accountId = _appSettings.FiksIOConfig.FiksIoAccountId;
            Log.Information($"Starting FiksIOReceiveAndReplySubscriber subscribe on account {accountId}...");
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