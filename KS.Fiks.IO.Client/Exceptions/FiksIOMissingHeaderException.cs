using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOMissingHeaderException : Exception
    {
        public FiksIOMissingHeaderException(string message)
            : base(message)
        {
        }

        public FiksIOMissingHeaderException()
        {
        }

        public FiksIOMissingHeaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}