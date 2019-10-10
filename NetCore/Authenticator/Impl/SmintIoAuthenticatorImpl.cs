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
