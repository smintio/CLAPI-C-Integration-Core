using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator
{
    public interface ISmintIoAuthenticator
    {
        Task<InitAuthenticationResultModel> InitSmintIoAuthenticationAsync();
        Task FinalizeSmintIoAuthenticationAsync(string authorizationCode);

        Task RefreshSmintIoTokenAsync();
    }
}
