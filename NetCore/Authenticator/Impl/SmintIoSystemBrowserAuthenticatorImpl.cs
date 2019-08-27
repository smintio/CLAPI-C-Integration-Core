using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Browser;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoSystemBrowserAuthenticatorImpl : SmintIoAuthenticatorImpl
    {
        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;

        private readonly ILogger _logger;

        public SmintIoSystemBrowserAuthenticatorImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,  
            ITokenDatabaseProvider tokenDatabaseProvider,
            ILogger<SmintIoSystemBrowserAuthenticatorImpl> logger)
            : base(settingsDatabaseProvider, tokenDatabaseProvider, logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _tokenDatabaseProvider = tokenDatabaseProvider;

            _logger = logger;
        }

        public override async Task<InitAuthenticationResultModel> InitSmintIoAuthenticationAsync()
        {
            _logger.LogInformation("Authenticating with Smint.io through system browser...");

            try
            {
                var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();

                settingsDatabaseModel.ValidateForAuthenticator();

                var authority = $"https://{settingsDatabaseModel.TenantId}.smint.io/.well-known/openid-configuration";

                var clientOptions = new OidcClientOptions
                {
                    Authority = authority,
                    ClientId = settingsDatabaseModel.ClientId,
                    ClientSecret = settingsDatabaseModel.ClientSecret,
                    Scope = "smintio.full openid profile offline_access",
                    RedirectUri = settingsDatabaseModel.RedirectUri,
                    FilterClaims = false,
                    Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                    ResponseMode = OidcClientOptions.AuthorizeResponseMode.FormPost
                };

                clientOptions.Policy.Discovery.ValidateIssuerName = false;
                clientOptions.Policy.ValidateTokenIssuerName = false;

                var result = await LoginAsync(settingsDatabaseModel.RedirectUri, clientOptions);

                var tokenDatabaseModel = await _tokenDatabaseProvider.GetTokenDatabaseModelAsync();

                tokenDatabaseModel.Success = !result.IsError;
                tokenDatabaseModel.ErrorMessage = result.Error;
                tokenDatabaseModel.AccessToken = result.AccessToken;
                tokenDatabaseModel.RefreshToken = result.RefreshToken;
                tokenDatabaseModel.IdentityToken = result.IdentityToken;
                tokenDatabaseModel.Expiration = result.AccessTokenExpiration;

                await _tokenDatabaseProvider.SetTokenDatabaseModelAsync(tokenDatabaseModel);

                if (!tokenDatabaseModel.Success)
                    throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.CannotAcquireSmintIoToken, 
                        $"Acquiring the Smint.io access token failed: {tokenDatabaseModel.ErrorMessage}");

                _logger.LogInformation("Successfully authenticated with Smint.io through system browser");

                // we need nothing else

                return new InitAuthenticationResultModel()
                {
                    Uuid = null,
                    RedirectUrl = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Smint.io through system browser");

                throw;
            }
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public override async Task FinalizeSmintIoAuthenticationAsync(string authorizationCode)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            // we need nothing else
        }

        private async Task<LoginResult> LoginAsync(string redirectUri, OidcClientOptions clientOptions)
        {
            IBrowser browser;

            if (int.TryParse(redirectUri.Substring(redirectUri.LastIndexOf(":") + 1), out int port))
            {
                browser = new SystemBrowser(port);
            }
            else
            {
                browser = new SystemBrowser();
            }

            clientOptions.Browser = browser;
            
            var client = new OidcClient(clientOptions);

            return await client.LoginAsync(new LoginRequest());
        }
    }
}
