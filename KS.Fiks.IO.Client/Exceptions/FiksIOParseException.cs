using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOParseException : Exception
    {
        public FiksIOParseException(string message)
            : base(message)
        {
        }

        public FiksIOParseException()
        {
        }

        public FiksIOParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}