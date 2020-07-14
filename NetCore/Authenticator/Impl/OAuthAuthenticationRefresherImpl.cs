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

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public abstract class OAuthAuthenticationRefresherImpl : IOAuthAuthenticationRefresher
    {
        private readonly IAuthenticationDatabaseProvider<TokenDatabaseModel> _tokenDatabaseProvider;

        private readonly ILogger<OAuthAuthenticationRefresherImpl> _logger;

        public Uri TokenEndPointUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public OAuthAuthenticationRefresherImpl(
            IAuthenticationDatabaseProvider<TokenDatabaseModel> tokenDatabaseProvider,
            ILogger<OAuthAuthenticationRefresherImpl> logger)
        {
            _tokenDatabaseProvider = tokenDatabaseProvider;
            _logger = logger;
        }

        public virtual async Task RefreshAuthenticationAsync()
        {
            _logger.LogInformation("Refreshing OAuth access token...");

            var _ = TokenEndPointUri?.ToString() ?? throw new ArgumentNullException("TokenEndpointUri");

            try
            { 
                var tokenDatabaseModel = await _tokenDatabaseProvider.GetAuthenticationDatabaseModelAsync().ConfigureAwait(false)
                                         ?? throw new NullReferenceException("No auth token data available to refresh");

                tokenDatabaseModel.ValidateForTokenRefresh();

                var client = new RestClient(TokenEndPointUri.ToString());
                var request = new RestRequest(Method.POST);

                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("refresh_token", tokenDatabaseModel.RefreshToken);
                request.AddParameter("client_id", ClientId);
                request.AddParameter("client_secret", ClientSecret);

                var response = await client.ExecuteAsync<RefreshTokenResultModel>(request).ConfigureAwait(false);
                var result = response.Data;

                tokenDatabaseModel = await _tokenDatabaseProvider.GetAuthenticationDatabaseModelAsync().ConfigureAwait(false);

                tokenDatabaseModel.Success = result.Success;
                tokenDatabaseModel.ErrorMessage = result.ErrorMsg;
                tokenDatabaseModel.AccessToken = result.AccessToken;
                tokenDatabaseModel.RefreshToken = result.RefreshToken;
                tokenDatabaseModel.IdentityToken = result.IdentityToken;
                tokenDatabaseModel.Expiration = result.Expiration;

                await _tokenDatabaseProvider.SetAuthenticationDatabaseModelAsync(tokenDatabaseModel).ConfigureAwait(false);

                if (!result.Success)
                {
                    throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotRefreshToken,
                        $"Refreshing the OAuth access token failed: {tokenDatabaseModel.ErrorMessage}");
                }

                _logger.LogInformation("Successfully refreshed access token for OAuth");
            }
            catch (AuthenticatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing access token for OAuth");

                throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotRefreshToken,
                        $"Refreshing the OAuth access token failed: {ex.Message}");
            }
        }

        public IAuthenticationDatabaseProvider<TokenDatabaseModel> GetAuthenticationDatabaseProvider() => _tokenDatabaseProvider;
    }
}
