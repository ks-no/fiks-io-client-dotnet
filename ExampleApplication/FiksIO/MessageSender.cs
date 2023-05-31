using System;
using System.Reflection;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using Serilog;

namespace ExampleApplication.FiksIO;

public class MessageSender
{
    private readonly IFiksIOClient _fiksIoClient;
    private readonly AppSettings _appSettings;
    
    private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);


    public MessageSender(IFiksIOClient fiksIoClient, AppSettings appSettings)
    {
        _fiksIoClient = fiksIoClient;
        _appSettings = appSettings;
    }

    public async Task<Guid> Send(string messageType, Guid toAccountId)
    {
        Log.Information("Seeeeeeeeeeend!!!!!!!!!!!");
        try
        {
            var sendtMessage = await _fiksIoClient
                .Send(new MeldingRequest(_appSettings.FiksIOConfig.FiksIoAccountId, toAccountId, messageType))
                .ConfigureAwait(false);
            return sendtMessage.MeldingId;
        }
        catch (Exception e)
        {
            Log.Error("MessageSender klarte ikke sende melding. Error: {ErrorMessage}", e.Message);
            throw;
        }
    }
}