using System;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal interface IAmqpReceiveConsumer : IAsyncBasicConsumer
    {
        event EventHandler<MottattMeldingArgs> Received;
    }
}