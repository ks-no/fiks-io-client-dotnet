using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpConsumerFactory
    {
        IAmqpReceiveConsumer CreateReceiveConsumer(IModel channel);
    }
}