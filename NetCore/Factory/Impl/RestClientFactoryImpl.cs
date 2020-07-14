using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization;
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

            return new RestClient
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer"),
            }.UseSerializer(() => new JsonNetSerializer());
        }

        private class JsonNetSerializer : IRestSerializer
        {
            public string Serialize(object obj) => 
                JsonConvert.SerializeObject(obj);

            public string Serialize(Parameter parameter) => 
                JsonConvert.SerializeObject(parameter.Value);

            public T Deserialize<T>(IRestResponse response) => 
                JsonConvert.DeserializeObject<T>(response.Content);

            public string[] SupportedContentTypes { get; } =
            {
                "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
            };

            public string ContentType { get; set; } = "application/json";

            public DataFormat DataFormat { get; } = DataFormat.Json;
        }
    }
}