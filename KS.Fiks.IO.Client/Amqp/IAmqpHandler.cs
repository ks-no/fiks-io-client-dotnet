using System;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpHandler
    {
        void AddReceivedListener(EventHandler<MessageReceivedArgs> receivedEvent);
    }
}