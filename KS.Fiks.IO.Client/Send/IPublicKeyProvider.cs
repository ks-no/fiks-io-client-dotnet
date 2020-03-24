using System;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Send
{
    public interface IPublicKeyProvider
    {
        Task<X509Certificate> GetPublicKey(Guid receiverAccountId);
    }
}