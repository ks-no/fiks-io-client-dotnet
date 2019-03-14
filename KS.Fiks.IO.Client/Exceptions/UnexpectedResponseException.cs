using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class UnexpectedResponseException : Exception
    {
        public UnexpectedResponseException()
        {
        }

        public UnexpectedResponseException(string message)
            : base(message)
        {
        }

        public UnexpectedResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}