using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class MissingHeaderException : Exception
    {
        public MissingHeaderException(string message)
            : base(message)
        {
        }

        public MissingHeaderException()
        {
        }

        public MissingHeaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}