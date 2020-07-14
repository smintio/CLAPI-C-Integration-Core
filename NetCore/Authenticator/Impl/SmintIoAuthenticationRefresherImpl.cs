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
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using System;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoAuthenticationRefresherImpl : OAuthAuthenticationRefresherImpl, ISmintIoAuthenticationRefresher
    {
        private readonly ISmintIoSettingsDatabaseProvider _smintIoSettingsDatabaseProvider;

        private ILogger<SmintIoAuthenticationRefresherImpl> _logger;

        public SmintIoAuthenticationRefresherImpl(
            ISmintIoSettingsDatabaseProvider smintIoSettingsDatabaseProvider,
            ISmintIoTokenDatabaseProvider tokenDatabaseProvider,
            ILogger<SmintIoAuthenticationRefresherImpl> logger)
            : base(tokenDatabaseProvider, logger)

        {
            _smintIoSettingsDatabaseProvider = smintIoSettingsDatabaseProvider;

            _logger = logger;
        }

        public override async Task RefreshAuthenticationAsync()
        {
            try
            {
                var smintIoSettingsDatabaseModel = await _smintIoSettingsDatabaseProvider.GetSmintIoSettingsDatabaseModelAsync().ConfigureAwait(false);

                smintIoSettingsDatabaseModel.ValidateForAuthenticator();

                TokenEndPointUri = new Uri($"https://{smintIoSettingsDatabaseModel.TenantId}.smint.io/connect/token");
                ClientId = smintIoSettingsDatabaseModel.ClientId;
                ClientSecret = smintIoSettingsDatabaseModel.ClientSecret;

                await base.RefreshAuthenticationAsync();
            } 
            catch (AuthenticatorException e)
            {
                throw new SmintIoAuthenticatorException(e.Error, e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Smint.io OAuth access token");

                throw new SmintIoAuthenticatorException(AuthenticatorException.AuthenticatorError.CannotRefreshToken,
                        $"Refreshing the Smint.io OAuth access token failed: {ex.Message}");
            }
        }
    }
}
