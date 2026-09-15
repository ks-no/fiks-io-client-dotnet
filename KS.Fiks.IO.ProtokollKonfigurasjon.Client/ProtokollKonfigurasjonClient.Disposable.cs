using System;

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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
