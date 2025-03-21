using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KS.Fiks.IO.Client.Amqp
{
    internal interface IAmqpReceiveConsumer : IAsyncBasicConsumer
    {
        event Func<MottattMeldingArgs, Task> ReceivedAsync;

        event Func<ConsumerEventArgs, Task> ConsumerCancelledAsync;
    }
}