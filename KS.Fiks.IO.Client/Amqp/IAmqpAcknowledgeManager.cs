using System;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpAcknowledgeManager
    {
        Task AckAsync();

        Task NackAsync();

        Task NackWithRequeueAsync();
    }

    public class AmqpAcknowledgeManager : IAmqpAcknowledgeManager
    {
        private readonly Func<Task> _ackAction;
        private readonly Func<Task> _nackAction;
        private readonly Func<Task> _nackWithRequeueAction;

        public AmqpAcknowledgeManager(Func<Task> ackAction, Func<Task> nackAction, Func<Task> nackWithRequeueAction)
        {
            _ackAction = ackAction;
            _nackAction = nackAction;
            _nackWithRequeueAction = nackWithRequeueAction;
        }

        public async Task AckAsync()
        {
            await _ackAction.Invoke().ConfigureAwait(false);
        }

        public async Task NackAsync()
        {
            await _nackAction.Invoke().ConfigureAwait(false);
        }

        public async Task NackWithRequeueAsync()
        {
            await _nackWithRequeueAction.Invoke().ConfigureAwait(false);
        }
    }
}