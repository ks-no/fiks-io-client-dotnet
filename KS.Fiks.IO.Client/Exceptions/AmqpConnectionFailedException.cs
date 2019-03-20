using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class AmqpConnectionFailedException : Exception
    {
        public AmqpConnectionFailedException(string message)
            : base(message)
        {
        }

        public AmqpConnectionFailedException()
        {
        }

        public AmqpConnectionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}