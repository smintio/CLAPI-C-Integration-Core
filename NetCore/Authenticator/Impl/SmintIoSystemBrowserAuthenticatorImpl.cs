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

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoSystemBrowserAuthenticatorImpl : SmintIoAuthenticatorImpl
    {
        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly IOAuthAuthenticator _oAuthAuthenticator;
        private readonly ILogger _logger;

        public SmintIoSystemBrowserAuthenticatorImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            IOAuthAuthenticator oAuthAuthenticator,
            ILogger<SmintIoSystemBrowserAuthenticatorImpl> logger)
            : base(settingsDatabaseProvider, oAuthAuthenticator, logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _oAuthAuthenticator = oAuthAuthenticator;
            _logger = logger;
        }

        public async Task InitSmintIoAuthenticationAsync()
        {
            _logger.LogInformation("Authenticating with Smint.io through system browser...");

            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync().ConfigureAwait(false);

            settingsDatabaseModel.ValidateForAuthenticator();

            _oAuthAuthenticator.AuthorityEndpoint = new Uri($"https://{settingsDatabaseModel.TenantId}.smint.io/.well-known/openid-configuration");
            _oAuthAuthenticator.ClientId = settingsDatabaseModel.ClientId;
            _oAuthAuthenticator.ClientSecret = settingsDatabaseModel.ClientSecret;
            _oAuthAuthenticator.Scope = "smintio.full openid profile offline_access";
            _oAuthAuthenticator.TargetRedirectionUrl = new Uri(settingsDatabaseModel.RedirectUri);

            await _oAuthAuthenticator.InitializeAuthenticationAsync().ConfigureAwait(false);

            _logger.LogInformation("Successfully authenticated with Smint.io through system browser");
        }
    }
}
