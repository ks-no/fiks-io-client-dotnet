using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOAmqpConnectionFailedException : Exception
    {
        public FiksIOAmqpConnectionFailedException(string message)
            : base(message)
        {
        }

        public FiksIOAmqpConnectionFailedException()
        {
        }

        public FiksIOAmqpConnectionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}