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
using RestSharp.Authenticators;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    /// <summary>
    /// Refreshes a JT token by re-authenticating with the remote service issuing the JWT token.
    /// <remarks>In contrary to OAuth, the user credentials are directly used to acquire a JWT token.
    /// JWT (or JSON web token) is just an encoding of the authentication data and not a complementing concept how to
    /// handle authentication process. With OAuth only the user knows its credentials. With JWT, it must be provided
    /// to the client requesting such a token.
    /// <para>Because of the reduced complexity, the JWT Authentication refresher can be used as authenticator too,
    /// as refreshing is the same operation as requesting a JWT token initially.</para></remarks>
    /// </summary>
    public class JwtAuthenticatorImpl : IAuthenticator<RemoteAuthDatabaseModel>
    {
        private readonly IRemoteAuthDatabaseProvider<RemoteAuthDatabaseModel> _remoteAuthDataProvider;
        private readonly ILogger<JwtAuthenticatorImpl> _logger;

        private JwtAuthOptions AuthenticationOptions { get; }


        public JwtAuthenticatorImpl(
            IRemoteAuthDatabaseProvider<RemoteAuthDatabaseModel> remoteAuthDataProvider,
            ILogger<JwtAuthenticatorImpl> logger,
            JwtAuthOptions authOptions)
        {
            _remoteAuthDataProvider = remoteAuthDataProvider;
            _logger = logger;
            AuthenticationOptions = authOptions;
        }

        public virtual async Task RefreshAuthenticationAsync()
        {
            var _ = AuthenticationOptions?.JwtTokenEndpoint
                    ?? throw new NullReferenceException("No token endpoint defined!");

            _logger.LogInformation("Refreshing OAuth token for remote system");

            var client = new RestClient(AuthenticationOptions?.JwtTokenEndpoint.ToString());
            var request = new RestRequest(AuthenticationOptions.UsePost ? Method.POST : Method.GET);

            client.Authenticator = new HttpBasicAuthenticator(
                AuthenticationOptions?.ClientId,
                AuthenticationOptions?.ClientSecret ?? String.Empty
            );

            request.AddParameter("grant_type", "password");
            request.AddParameter("username", AuthenticationOptions?.Username);
            request.AddParameter("password", AuthenticationOptions?.Password);

            var response = await client.ExecuteAsync<RefreshTokenResultModel>(request).ConfigureAwait(false);
            var result = response.Data;

            var authData = await _remoteAuthDataProvider.GetAuthenticationDataAsync().ConfigureAwait(false);

            authData.Success = result.Success;
            authData.ErrorMessage = result.ErrorMsg;
            authData.AuthData = result.AccessToken;
            authData.Expiration = result.Expiration;

            await _remoteAuthDataProvider.SetAuthenticationDataAsync(authData).ConfigureAwait(false);

            if (!result.Success)
            {
                throw new SmintIoAuthenticatorException(SmintIoAuthenticatorException.AuthenticatorError.CannotRefreshSmintIoToken,
                    $"Refreshing the JWT access token failed: {authData.ErrorMessage}");
            }

            _logger.LogInformation("Successfully refreshed token for OAuth");
        }

        public IAuthenticationDataProvider<RemoteAuthDatabaseModel> GetAuthenticationDataProvider()
            => _remoteAuthDataProvider;

        public Task InitializeAuthenticationAsync() => RefreshAuthenticationAsync();
    }
}
