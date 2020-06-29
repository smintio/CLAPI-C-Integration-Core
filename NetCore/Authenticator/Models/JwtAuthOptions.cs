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
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Models
{
    /// <summary>
    /// Provides authentication data required by <see cref="JwtAuthenticatorImpl"/>
    /// <remarks>JWT tokens are used with any type fo HTTP authentication, like "basic", "digest" or form based
    /// authentication. The initial authentication and authorization will be used to request a JWT token from a token
    /// endpoint URL. So usually no user interaction takes place.
    /// The authentication credentials are provided by instances of this class.
    /// <para>Some times the credentials are submitted as form data additionally, at other times additionally
    /// credentials have to be submitted as POST form body. Therefore, two schemes are provided here:</para>
    /// <list type="bullet">
    /// <item><c>ClientId</c>/<c>ClientSecret</c> - credentials that are used with HTTP authentication header.
    /// If only POST request form data is used with the token endpoint, just omit these data.
    /// </item>
    /// <item><c>Username</c>/<c>Password</c> - credentials sent as form data within as HTTP POST body.
    /// If only <c>GET</c> requests are used or no POST body is in use, then just omit these values.
    /// </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public class JwtAuthOptions
    {
        public JwtAuthOptions() { }

        public Uri JwtTokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool UsePost { get; set; } = true;
    }
}
