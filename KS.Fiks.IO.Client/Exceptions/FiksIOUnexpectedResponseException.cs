using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOUnexpectedResponseException : Exception
    {
        public FiksIOUnexpectedResponseException()
        {
        }

        public FiksIOUnexpectedResponseException(string message)
            : base(message)
        {
        }

        public FiksIOUnexpectedResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}