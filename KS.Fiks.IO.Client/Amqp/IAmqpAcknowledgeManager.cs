using System;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpAcknowledgeManager
    {
        Task AckAsync();

        Task NackAsync();

        Task RejectWithRequeueAsync();

        Task NackWithRequeueAsync();
    }

    public class AmqpAcknowledgeManager : IAmqpAcknowledgeManager
    {
        private readonly Func<Task> _ackAction;
        private readonly Func<Task> _nackAction;
        private readonly Func<Task> _rejectWithRequeueAction;

        public AmqpAcknowledgeManager(Func<Task> ackAction, Func<Task> nackAction, Func<Task> rejectWithRequeueAction)
        {
            _ackAction = ackAction;
            _nackAction = nackAction;
            _rejectWithRequeueAction = rejectWithRequeueAction;
        }

        public async Task AckAsync()
        {
            await _ackAction.Invoke().ConfigureAwait(false);
        }

        public async Task NackAsync()
        {
            await _nackAction.Invoke().ConfigureAwait(false);
        }

        public async Task RejectWithRequeueAsync()
        {
            await _rejectWithRequeueAction.Invoke().ConfigureAwait(false);
        }

        public async Task NackWithRequeueAsync()
        {
            await _rejectWithRequeueAction.Invoke().ConfigureAwait(false);
        }
    }
}
