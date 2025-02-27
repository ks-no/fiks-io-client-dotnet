using System;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpAcknowledgeManager
    {
        Func<Task> Ack();

        Func<Task> Nack();

        Func<Task> NackWithRequeue();
    }

    public class AmqpAcknowledgeManager : IAmqpAcknowledgeManager
    {
        private readonly Func<Task> _ackAction;
        private readonly Func<Task> _nackAction;
        private readonly Func<Task> _nackWithRequeueAction;

        public AmqpAcknowledgeManager(Func<Task> ackAction, Func<Task> nackAction, Func<Task> nackWithRequeueAction)
        {
            _ackAction = ackAction ?? throw new ArgumentNullException(nameof(ackAction));
            _nackAction = nackAction ?? throw new ArgumentNullException(nameof(nackAction));
            _nackWithRequeueAction = nackWithRequeueAction ?? throw new ArgumentNullException(nameof(nackWithRequeueAction));
        }

        public Func<Task> Ack()
        {
            return _ackAction;
        }

        public Func<Task> Nack()
        {
            return _nackAction;
        }

        public Func<Task> NackWithRequeue()
        {
            return _nackWithRequeueAction;
        }
    }
}