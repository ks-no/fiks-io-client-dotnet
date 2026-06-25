using System;
using System.Net.Http;
using System.Threading.Tasks;
using KS.Fiks.IO.ProtokollKonfigurasjon.Client.Generated;

namespace KS.Fiks.IO.ProtokollKonfigurasjon.Client
{
    public static class KontoKonfigurasjonKlient
    {
        public static IProtokollKonfigurasjonClient CreateKlient(
            string hostUrl,
            Guid integrasjonId,
            string integrasjonPassord,
            Func<Task<string>> maskinportenTokenSupplier)
        {
            var httpClient = new HttpClient(new FiksIntegrasjonHttpMessageHandler(integrasjonId, integrasjonPassord, maskinportenTokenSupplier));
            return new ProtokollKonfigurasjonClient(httpClient) { BaseUrl = hostUrl };
        }
    }
}
