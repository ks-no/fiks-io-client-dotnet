using System;
using System.Net.Http;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    // Hand-written extension of the NSwag-generated client/interface. Kept in a separate,
    // non-generated file (outside Generated/, which is gitignored) so it survives regeneration
    // of ProtokollKonfigurasjonClient.g.cs. Adds IDisposable so callers can release the underlying
    // HttpClient created for them by KontoKonfigurasjonClient.CreateClient, since nswag.json sets
    // disposeHttpClient=false.
    public partial interface IProtokollKonfigurasjonClient : IDisposable
    {
    }

    public partial class ProtokollKonfigurasjonClient : IDisposable
    {
        private bool _ownsHttpClient;
        private bool _disposed;

        // Used by KontoKonfigurasjonClient.CreateClient, which creates the HttpClient itself and
        // therefore hands over ownership of it. Callers using the generated public constructor
        // inject their own (possibly shared) HttpClient and keep ownership of it, so disposing
        // this client must leave theirs alone.
        internal ProtokollKonfigurasjonClient(HttpClient httpClient, bool ownsHttpClient)
            : this(httpClient)
        {
            _ownsHttpClient = ownsHttpClient;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _ownsHttpClient)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
