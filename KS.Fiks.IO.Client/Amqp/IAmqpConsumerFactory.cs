using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal interface IAmqpConsumerFactory
    {
        IAmqpReceiveConsumer CreateReceiveConsumer(IChannel channel);
    }
}