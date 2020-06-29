using System;
using System.Runtime.ConstrainedExecution;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpAcknowledgeManager
    {
        Action Ack();

        Action Nack();

        Action NackWithRequeue();
    }

    public class AmqpAcknowledgeManager : IAmqpAcknowledgeManager
    {
        private readonly Action _ackAction;
        private readonly Action _nackAction;
        private readonly Action _nackWithRequeueAction;

        public AmqpAcknowledgeManager(Action ackAction, Action nackAction, Action nackWithRequeueAction)
        {
            this._ackAction = ackAction;
            this._nackAction = nackAction;
            this._nackWithRequeueAction = nackWithRequeueAction;
        }

        public Action Ack()
        {
            if (this._ackAction == null)
            {
                throw new NotSupportedException("Ack is currently not supported");
            }

            return this._ackAction;
        }

        public Action Nack()
        {
            if (this._nackAction == null)
            {
                throw new NotSupportedException("Nack is currently not supported");
            }

            return this._nackAction;
        }

        public Action NackWithRequeue()
        {
            if (this._nackWithRequeueAction == null)
            {
                throw new NotSupportedException("Nack is currently not supported");
            }

            return this._nackWithRequeueAction;
        }
    }
}