using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Fiks.Svarinn.Client.Maskinporten
{
    public interface IMaskinportenClient
    {
        Task<string> GetAccessToken(ICollection<string> scopes);
    }
}