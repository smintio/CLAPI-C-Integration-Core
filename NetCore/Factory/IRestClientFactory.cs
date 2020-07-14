using System.Threading.Tasks;
using RestSharp;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Factory
{
    /// <summary>
    /// Factory interface to instantiate a <see cref="IRestClient"/>
    /// </summary>
    public interface IRestClientFactory
    {
        Task<IRestClient> CreateRestClientAsync();
    }
}