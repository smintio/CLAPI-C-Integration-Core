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
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoAuthenticationRefresherImpl : ISmintIoAuthenticationRefresher
    {
        private readonly IOAuthAuthenticationRefresher _oAuthAuthenticator;

        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ILogger<SmintIoAuthenticationRefresherImpl> _logger;

        public SmintIoAuthenticationRefresherImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            IOAuthAuthenticationRefresher oAuthAuthenticator,
            ILogger<SmintIoAuthenticationRefresherImpl> logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _logger = logger;
            _oAuthAuthenticator = oAuthAuthenticator;
        }

        public virtual Task RefreshSmintIoTokenAsync()
        {
            return RefreshAuthenticationAsync();
        }

        public async Task RefreshAuthenticationAsync()
        {
            _logger.LogInformation("Refreshing token for Smint.io");

            await ConfigureOAuthAsync().ConfigureAwait(false);
            await _oAuthAuthenticator.RefreshAuthenticationAsync();

            _logger.LogInformation("Successfully refreshed token for Smint.io");
        }

        public IAuthenticationDataProvider<TokenDatabaseModel> GetAuthenticationDataProvider()
        {
            return _oAuthAuthenticator.GetAuthenticationDataProvider();
        }

        private async Task ConfigureOAuthAsync()
        {
            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync().ConfigureAwait(false);

            settingsDatabaseModel.ValidateForAuthenticator();

            _oAuthAuthenticator.TokenEndPointUri = new Uri($"https://{settingsDatabaseModel.TenantId}.smint.io/connect/token");
            _oAuthAuthenticator.ClientId = settingsDatabaseModel.ClientId;
            _oAuthAuthenticator.ClientSecret = settingsDatabaseModel.ClientSecret;
        }
    }
}
