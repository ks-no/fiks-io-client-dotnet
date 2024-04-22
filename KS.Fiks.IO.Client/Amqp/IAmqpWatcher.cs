using System;

namespace KS.Fiks.IO.Client.Amqp
{
    public interface IAmqpWatcher
    {
        void HandleConnectionBlocked(object sender, EventArgs e);
        void HandleConnectionShutdown(object sender, EventArgs shutdownEventArgs);
        void HandleConnectionUnblocked(object sender, EventArgs e);
    }
}