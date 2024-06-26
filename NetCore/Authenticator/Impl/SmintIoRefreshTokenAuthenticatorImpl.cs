﻿#region copyright
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
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using System.Net.Http;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl
{
    public class SmintIoRefreshTokenAuthenticatorImpl : OAuthAuthenticatorImpl, ISmintIoAuthenticator
    {
        private readonly ISmintIoSettingsDatabaseProvider _smintIoSettingsDatabaseProvider;

        private readonly ILogger<SmintIoSystemBrowserAuthenticatorImpl> _logger;

        public SmintIoRefreshTokenAuthenticatorImpl(
            ISmintIoSettingsDatabaseProvider smintIoSettingsDatabaseProvider,
            ISmintIoTokenDatabaseProvider tokenDatabaseProvider,
            IHttpClientFactory httpClientFactory,
            ILogger<SmintIoSystemBrowserAuthenticatorImpl> logger)
            : base(tokenDatabaseProvider, httpClientFactory, logger)
        {
            _smintIoSettingsDatabaseProvider = smintIoSettingsDatabaseProvider;

            _logger = logger;
        }

        public override async Task InitializeAuthenticationAsync()
        { 
            try
            {
                var smintIoSettingsDatabaseModel = await _smintIoSettingsDatabaseProvider.GetSmintIoSettingsDatabaseModelAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(smintIoSettingsDatabaseModel.RefreshToken))
                    throw new Exception("No refresh token present in settings");

                smintIoSettingsDatabaseModel.ValidateForAuthenticator();

                TokenEndPointUri = new Uri($"https://{smintIoSettingsDatabaseModel.TenantId}.smint.io/connect/token");
                ClientId = smintIoSettingsDatabaseModel.ClientId;
                ClientSecret = smintIoSettingsDatabaseModel.ClientSecret;
                RefreshToken = smintIoSettingsDatabaseModel.RefreshToken;

                await base.RefreshAuthenticationAsync().ConfigureAwait(false);
            }
            catch (AuthenticatorException e)
            {
                throw new SmintIoAuthenticatorException(e.Error, e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring Smint.io OAuth access token through refresh token");

                throw new SmintIoAuthenticatorException(AuthenticatorException.AuthenticatorError.CannotAcquireToken,
                        $"Acquiring the Smint.io OAuth access token through refresh token failed: {ex.Message}");
            }
        }
    }
}
