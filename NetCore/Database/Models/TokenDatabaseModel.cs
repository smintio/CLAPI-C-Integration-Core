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

using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Models
{
    public class TokenDatabaseModel
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdentityToken { get; set; }

        public DateTimeOffset? Expiration { get; set; }

        internal void ValidateForTokenRefresh()
        {
            if (!Success || string.IsNullOrEmpty(RefreshToken)) throw new ArgumentNullException(nameof(RefreshToken));
        }

        internal void ValidateForSync()
        {
            if (!Success || string.IsNullOrEmpty(AccessToken)) throw new ArgumentNullException(nameof(AccessToken));
        }

        internal void ValidateForPusher()
        {
            ValidateForSync();
        }
    }
}
