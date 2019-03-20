using System;
using KS.Fiks.IO.Client.Models;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpReceiveConsumer : IBasicConsumer
    {
        event EventHandler<MessageReceivedArgs> Received;
    }
}