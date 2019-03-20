using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException()
        {
        }

        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}