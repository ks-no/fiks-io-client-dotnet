using System.Threading.Tasks;
using KS.Fiks.IO.Client.Models;

namespace KS.Fiks.IO.Client
{
    public interface ICatalogHandler
    {
        Task<Account> Lookup(LookupRequest request);
    }
}