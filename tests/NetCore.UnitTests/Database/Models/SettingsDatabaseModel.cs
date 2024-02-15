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
using System.Diagnostics.CodeAnalysis;
using Xunit;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.UnitTests.Database.Models
{
    [ExcludeFromCodeCoverage]
    public class SettingsDatabaseModelTest
    {
        [Fact]
        public void ValidateForAuthenticator_success()
        {
            CreateValidSettingsDatabaseModel().ValidateForAuthenticator();
            Assert.True(true);
        }
        
        [Fact]
        public void ValidateForSync_success()
        {
            CreateValidSettingsDatabaseModel().ValidateForSync();
            Assert.True(true);
        }
        
        [Fact]
        public void ValidateForPusher_success()
        {
            CreateValidSettingsDatabaseModel().ValidateForPusher();
            Assert.True(true);
        }
        
        [Fact]
        public void ValidateForAuthenticator_noTenantId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.TenantId = null;
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The tenant ID is missing", exc.Message);
        }
        
        [Fact]
        public void ValidateForAuthenticator_emptyTenantId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.TenantId = "";
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The tenant ID is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_noClientId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ClientId = null;
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The client ID is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_emptyClientId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ClientId = "";
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The client ID is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_noClientSecret()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ClientSecret = null;
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The client secret is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_emptyClientSecret()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ClientSecret = "";
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The client secret is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_noRedirectUri()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.RedirectUri = null;
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The redirect URI is missing", exc.Message);
        }

        [Fact]
        public void ValidateForAuthenticator_emptyRedirectUri()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.RedirectUri = "";
                dbModel.ValidateForAuthenticator();
            });
            Assert.Equal("The redirect URI is missing", exc.Message);
        }

        [Fact]
        public void ValidateForSync_noTenantId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.TenantId = null;
                dbModel.ValidateForSync();
            });
            Assert.Equal("The tenant ID is missing", exc.Message);
        }
        
        [Fact]
        public void ValidateForSync_emptyTenantId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.TenantId = "";
                dbModel.ValidateForSync();
            });
            Assert.Equal("The tenant ID is missing", exc.Message);
        }

        [Fact]
        public void ValidateForSync_noImportLanguages()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ImportLanguages = null;
                dbModel.ValidateForSync();
            });
            Assert.Equal("The import languages are missing", exc.Message);
        }
        
        [Fact]
        public void ValidateForSync_emptyImportLanguages()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ImportLanguages = new string[] {};
                dbModel.ValidateForSync();
            });
            Assert.Equal("The import languages are missing", exc.Message);
        }

        [Fact]
        public void ValidateForSync_noChannelId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ChannelId = null;
                dbModel.ValidateForPusher();
            });
            Assert.Equal("The channel ID is missing", exc.Message);
        }
        
        [Fact]
        public void ValidateForSync_emptyChannelId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ChannelId = 0;
                dbModel.ValidateForPusher();
            });
            Assert.Equal("The channel ID is invalid: 0", exc.Message);
        }

        [Fact]
        public void ValidateForSync_negativeChannelId()
        {
            SmintIoAuthenticatorException exc = CheckForException((dbModel) =>
            {
                dbModel.ChannelId = -17;
                dbModel.ValidateForPusher();
            });
            Assert.Equal("The channel ID is invalid: -17", exc.Message);
        }


        private SmintIoSettingsDatabaseModel CreateValidSettingsDatabaseModel()
        {
            return new SmintIoSettingsDatabaseModel()
            {
                ChannelId = 1,
                TenantId = "tennantId",
                ClientId = "clientId",
                ClientSecret = "SECRET",
                RedirectUri = "file://",
                ImportLanguages = new string[] { "en" }
            };
        }

        private SmintIoAuthenticatorException CheckForException(Action<SmintIoSettingsDatabaseModel> prepareData)
        {
            SmintIoSettingsDatabaseModel dbModel = CreateValidSettingsDatabaseModel();
            return Assert.Throws<SmintIoAuthenticatorException>(() => prepareData(dbModel));
        }
    }
}
