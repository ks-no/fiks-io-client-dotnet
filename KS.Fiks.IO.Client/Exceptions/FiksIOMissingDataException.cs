using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIOMissingDataException : Exception
    {
        public FiksIOMissingDataException(string message)
            : base(message)
        {
        }

        public FiksIOMissingDataException()
        {
        }

        public FiksIOMissingDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}