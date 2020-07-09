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

#nullable enable
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator
{
    /// <summary>
    /// An authenticator for synchronization targets.
    /// </summary>
    /// <remarks>Since .NET dependency injector does not support named injections because it favors strong typing of
    /// injected instances, this interface is used to mark authenticators used for the sync target. Using this
    /// interface with the dependency injector provides both: strong typing and selecting sync target specific
    /// authenticator.</remarks>
    public interface ISyncTargetAuthenticator
    {
        /// <summary>
        /// Refresh the authentication data with remote systems (see (<see cref="IAuthenticator{T}"/>)).
        /// </summary>
        /// <returns>A task to wait for finishing or not.</returns>
        Task RefreshAuthenticationAsync();

        /// <summary>
        /// Performs an authentication with the remote system.
        /// </summary>
        /// <remarks>All required data to perform the authentication must be provided elsewhere as it is implementation
        /// dependent, what is needed. (<see cref="IAuthenticator{T}"/>)</remarks>
        /// <returns>A task to wait for finishing the process or not.</returns>
        Task InitializeAuthenticationAsync();

        /// <summary>
        /// Returns the access token that can be used to call the target API directly.
        /// </summary>
        /// <remarks>A valid value is returned only if the authentication scheme uses access tokens. The kind of
        /// access token is target dependent and thus does not need to be provided here.</remarks>
        /// <returns>An access token or <c>null</c> if none is used nor available.</returns>
        Task<string?> GetAccessTokenAsync();
    }
}
