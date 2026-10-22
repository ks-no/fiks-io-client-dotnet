using System;

namespace KS.Fiks.IO.Client
{
    public class FiksIOMaskinportenTokenException : Exception
    {
        public FiksIOMaskinportenTokenException()
        {
        }

        public FiksIOMaskinportenTokenException(string message) : base(message)
        {
        }

        public FiksIOMaskinportenTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}