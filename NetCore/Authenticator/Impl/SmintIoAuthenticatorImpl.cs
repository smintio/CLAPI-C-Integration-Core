using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public abstract class SmintIoAuthenticatorImpl : ISmintIoAuthenticator
    {
        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;

        private readonly ILogger<SmintIoAuthenticatorImpl> _logger;
        
        public SmintIoAuthenticatorImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            ITokenDatabaseProvider tokenDatabaseProvider,
            ILogger<SmintIoAuthenticatorImpl> logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _tokenDatabaseProvider = tokenDatabaseProvider;

            _logger = logger;
        }

        public abstract Task<InitAuthenticationResultModel> InitSmintIoAuthenticationAsync();
        public abstract Task FinalizeSmintIoAuthenticationAsync(string authorizationCode);

        public async Task RefreshSmintIoTokenAsync()
        {
            _logger.LogInformation("Refreshing token for Smint.io");
            
            var tokenDatabaseModel = await _tokenDatabaseProvider.GetTokenDatabaseModelAsync();

            tokenDatabaseModel.ValidateForTokenRefresh();

            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();

            settingsDatabaseModel.ValidateForAuthenticator();
            
            var tokenEndpoint = $"https://{settingsDatabaseModel.TenantId}.smint.io/connect/token";

            var client = new RestClient(tokenEndpoint);

            var request = new RestRequest(Method.POST);

            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", tokenDatabaseModel.RefreshToken);
            request.AddParameter("client_id", settingsDatabaseModel.ClientId);
            request.AddParameter("client_secret", settingsDatabaseModel.ClientSecret);

            var response = await client.ExecuteTaskAsync<RefreshTokenResultModel>(request);

            var result = response.Data;

            tokenDatabaseModel = await _tokenDatabaseProvider.GetTokenDatabaseModelAsync();

            tokenDatabaseModel.Success = result.Success;
            tokenDatabaseModel.ErrorMessage = result.ErrorMsg;
            tokenDatabaseModel.AccessToken = result.AccessToken;
            tokenDatabaseModel.RefreshToken = result.RefreshToken;
            tokenDatabaseModel.IdentityToken = result.IdentityToken;
            tokenDatabaseModel.Expiration = result.Expiration;            

            await _tokenDatabaseProvider.SetTokenDatabaseModelAsync(tokenDatabaseModel);

            if (!result.Success)
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.CannotRefreshSmintIoToken, 
                    $"Refreshing the Smint.io access token failed: {tokenDatabaseModel.ErrorMessage}");

            _logger.LogInformation("Successfully refreshed token for Smint.io");
        }
    }
}
