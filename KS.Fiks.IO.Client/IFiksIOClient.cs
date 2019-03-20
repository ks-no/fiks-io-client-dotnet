using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client
{
    public interface IFiksIOClient
    {
        string AccountId { get; }

        Task<Account> Lookup(LookupRequest request);

        Task<SentMessage> Send(MessageRequest request, IEnumerable<IPayload> payload);

        Task<SentMessage> Send(MessageRequest request, string pathToPayload);

        Task<SentMessage> Send(MessageRequest request, string payload, string filename);

        Task<SentMessage> Send(MessageRequest request, Stream payload, string filename);

        void NewSubscription(EventHandler<MessageReceivedArgs> onReceived);

        void NewSubscription(EventHandler<MessageReceivedArgs> onReceived, EventHandler<ConsumerEventArgs> onCanceled);
    }
}
