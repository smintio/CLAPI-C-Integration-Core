#region copyright
// MIT License
//
// Copyright (c) 2019 Smint.io GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice (including the next paragraph) shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// SPDX-License-Identifier: MIT
#endregion

using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Browser;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class OAuthAuthenticatorImpl : OAuthAuthenticationRefresherImpl, IOAuthAuthenticator
    {
        private readonly ILogger<OAuthAuthenticatorImpl> _logger;

        public Uri AuthorityEndpoint { get; set; }

        public string Scope { get; set; }

        public Uri TargetRedirectionUrl { get; set; }

        public OAuthAuthenticatorImpl(
            IAuthenticationDatabaseProvider<TokenDatabaseModel> tokenDatabaseProvider,
            ILogger<OAuthAuthenticatorImpl> logger)
            : base(tokenDatabaseProvider, logger)
        {
            _logger = logger;
        }

        public virtual async Task InitializeAuthenticationAsync()
        {
            _logger.LogInformation("Acquiring OAuth access token through system browser...");

            var authority = AuthorityEndpoint?.ToString() ?? throw new ArgumentNullException("AuthorityEndpoint");

            try
            {
                var clientOptions = new OidcClientOptions
                {
                    Authority = authority,
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    Scope = Scope,
                    RedirectUri = TargetRedirectionUrl?.ToString() ?? throw new ArgumentNullException("RedirectUri"),
                    FilterClaims = false
                };

                clientOptions.Policy.Discovery.ValidateIssuerName = false;
                clientOptions.Policy.ValidateTokenIssuerName = false;

                var result = await LoginAsync(TargetRedirectionUrl, clientOptions).ConfigureAwait(false);

                var tokenDatabaseModel = await GetAuthenticationDatabaseProvider().GetAuthenticationDatabaseModelAsync().ConfigureAwait(false);

                tokenDatabaseModel.Success = !result.IsError;
                tokenDatabaseModel.ErrorMessage = result.Error;
                tokenDatabaseModel.AccessToken = result.AccessToken;
                tokenDatabaseModel.RefreshToken = result.RefreshToken;
                tokenDatabaseModel.IdentityToken = result.IdentityToken;
                tokenDatabaseModel.Expiration = result.AccessTokenExpiration;

                await GetAuthenticationDatabaseProvider().SetAuthenticationDatabaseModelAsync(tokenDatabaseModel).ConfigureAwait(false);

                if (!tokenDatabaseModel.Success)
                {
                    throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotAcquireToken,
                        $"Acquiring the OAuth access token failed: {tokenDatabaseModel.ErrorMessage}");
                }

                _logger.LogInformation("Successfully acquired OAuth access token through system browser");
            }
            catch (AuthenticatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring OAuth access token through system browser");

                throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotAcquireToken,
                        $"Acquiring the OAuth access token through system browser failed: {ex.Message}");
            }
        }

        private async Task<LoginResult> LoginAsync(Uri redirectUri, OidcClientOptions clientOptions)
        {
            IBrowser browser;

            if (!redirectUri.IsDefaultPort)
            {
                browser = new SystemBrowser(redirectUri.Port);
            }
            else
            {
                browser = new SystemBrowser();
            }

            clientOptions.Browser = browser;

            var client = new OidcClient(clientOptions);

            return await client.LoginAsync(new LoginRequest()).ConfigureAwait(false);
        }
    }
}
