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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class JwtAuthenticatorImpl : IJwtAuthenticator
    {
        private readonly ISyncTargetAuthenticationDatabaseProvider<SyncTargetAuthenticationDatabaseModel> _syncTargetAuthenticationDatabaseProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JwtAuthenticatorImpl> _logger;

        public JwtAuthOptions AuthenticationOptions { get; set;  }

        public JwtAuthenticatorImpl(
            ISyncTargetAuthenticationDatabaseProvider<SyncTargetAuthenticationDatabaseModel> syncTargetAuthenticationDatabaseProvider,
            JwtAuthOptions authOptions,
            IServiceProvider serviceProvider,
            ILogger<JwtAuthenticatorImpl> logger)
        {
            _syncTargetAuthenticationDatabaseProvider = syncTargetAuthenticationDatabaseProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
            AuthenticationOptions = authOptions;
        }

        public virtual async Task RefreshAuthenticationAsync()
        {
            _logger.LogInformation("Refreshing JWT token...");

            var requestUri = AuthenticationOptions?.JwtTokenEndpoint ?? throw new ArgumentNullException("JwtTokenEndpoint");

            try
            {
                var authenticationDatabaseModel = await _syncTargetAuthenticationDatabaseProvider.GetAuthenticationDatabaseModelAsync().ConfigureAwait(false)
                                     ?? throw new NullReferenceException("No authentication data available to refresh");

                var credentials = new BasicAuthenticationHeaderValue(
                    AuthenticationOptions?.ClientId, AuthenticationOptions?.ClientSecret ?? String.Empty
                );

                if (!AuthenticationOptions.UsePost)
                {
                    // add the user credential data to the query parameters
                    var queryData = QueryHelpers.ParseQuery(requestUri.Query);

                    var queryItems = queryData.SelectMany(
                        x => x.Value,
                        (col, value) => new KeyValuePair<string, string>(col.Key, value)
                    ).ToList();

                    requestUri = new UriBuilder(requestUri)
                    {
                        Query = new QueryBuilder(queryItems)
                        {
                            {"grant_type", "password"},
                            {"username", AuthenticationOptions?.Username},
                            {"password", AuthenticationOptions?.Password}
                        }.ToQueryString().ToString(),
                    }.Uri;
                }

                bool isAuthSuccessful;
                var method = AuthenticationOptions.UsePost ? HttpMethod.Post : HttpMethod.Get;

                using (var client = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient())
                {
                    using var request = new HttpRequestMessage(method, requestUri);
                    request.Headers.Authorization = credentials;

                    if (AuthenticationOptions.UsePost)
                    {
                        request.Content = CreatePostContent(AuthenticationOptions);
                    }

                    using var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseContentRead)
                        .ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    var responseData = JsonConvert.DeserializeObject<JwtRefreshTokenResultModel>(
                        await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    );

                    isAuthSuccessful = responseData.IsSuccess();

                    authenticationDatabaseModel.Success = isAuthSuccessful;
                    authenticationDatabaseModel.ErrorMessage = responseData.ErrorMsg;
                    authenticationDatabaseModel.AuthData = responseData.AccessToken;

                    await _syncTargetAuthenticationDatabaseProvider.SetAuthenticationDatabaseModelAsync(authenticationDatabaseModel).ConfigureAwait(false);
                }

                if (!isAuthSuccessful)
                {
                    throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotRefreshToken,
                        $"Refreshing the JWT access token failed: {authenticationDatabaseModel.ErrorMessage}");
                }

                _logger.LogInformation("Successfully refreshed token for JWT");
            }
            catch (AuthenticatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWT token");

                throw new AuthenticatorException(AuthenticatorException.AuthenticatorError.CannotRefreshToken,
                        $"Refreshing the JWT token failed: {ex.Message}");
            }
        }

        public IAuthenticationDatabaseProvider<SyncTargetAuthenticationDatabaseModel> GetAuthenticationDatabaseProvider()
            => _syncTargetAuthenticationDatabaseProvider;

        public async Task InitializeAuthenticationAsync() => await RefreshAuthenticationAsync();

        /// <summary>
        /// Creates POST body with form data encoding to simulate a form authentication.
        /// </summary>
        /// <remarks><para>Other implementation could use JSON instead.</para>
        /// <para>This is only called in case a POST request is performed as requested in the authentication
        /// options <see cref="JwtAuthOptions"/>.</para>
        /// <code>
        /// var json = JsonConvert.SerializeObject(new JsonObject()
        /// {
        ///     {"grant_type", "password"},
        ///     {"username", authOptions?.Username},
        ///     {"password", authOptions?.Password}
        /// });
        ///
        /// return new StringContent(json, Encoding.UTF8, "application/json");
        /// </code>
        /// </remarks>
        /// <param name="authOptions">The authentication options usable to create the request body.</param>
        /// <returns></returns>
        protected virtual HttpContent CreatePostContent(JwtAuthOptions authOptions)
        {
            return new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"grant_type", "password"},
                {"username", authOptions?.Username},
                {"password", authOptions?.Password}
            });
        }
    }
}
