using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using RestSharp.Serializers.NewtonsoftJson;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Factory
{
    public class RestClientFactoryImpl : IRestClientFactory
    {
        private readonly ISyncTargetAuthenticator _syncTargetAuthenticator;

        public RestClientFactoryImpl(ISyncTargetAuthenticator syncTargetAuthenticator)
        {
            _syncTargetAuthenticator = syncTargetAuthenticator;
        }

        public async Task<IRestClient> CreateRestClientAsync()
        {
            // create a new RestClient using the Canto Access Token as Bearer Token and a custom JsonSerializer.
            // the custom JsonSerializer is needed in order to be able to influence how enums are serialized.

            var accessToken = await _syncTargetAuthenticator.GetAccessTokenAsync();

            var restClientOptions = new RestClientOptions
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer")
            };

            var restClient = new RestClient(
                restClientOptions, 
                configureSerialization: s => s.UseNewtonsoftJson());

            return restClient;
        }
    }
}