using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOAmqpSetupFailedException : Exception
    {
        public FiksIOAmqpSetupFailedException(string message)
            : base(message)
        {
        }

        public FiksIOAmqpSetupFailedException()
        {
        }

        public FiksIOAmqpSetupFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}