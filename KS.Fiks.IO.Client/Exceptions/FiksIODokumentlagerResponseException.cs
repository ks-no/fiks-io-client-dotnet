using System;

namespace KS.Fiks.IO.Client.Exceptions
{
    public class FiksIODokumentlagerResponseException : Exception
    {
        public FiksIODokumentlagerResponseException(string message)
            : base(message)
        {
        }

        public FiksIODokumentlagerResponseException()
        {
        }

        public FiksIODokumentlagerResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}