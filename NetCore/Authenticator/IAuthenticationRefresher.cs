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

using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Authenticator
{
    /// <summary>
    /// Defines a refresher of authentication data to be used with remote systems to authenticate.
    /// </summary>
    /// <typeparam name="T">Defines the type of the authentication storage that is used with the authenticator.
    /// </typeparam>
    public interface IAuthenticationRefresher<T>
    {
        /// <summary>
        /// Refresh the authentication data with remote systems.
        /// </summary>
        /// <returns>A task to wait for finishing or not.</returns>
        Task RefreshAuthenticationAsync();

        /// <summary>
        /// Provides the token database provider that is used to store the authentication data of this authenticator.
        /// </summary>
        ///
        /// <remarks>Supporting multiple synchronising clients for multiple tenants and multiple targets may lead
        /// to insecurity, which token database provider really is used with the authenticators.
        /// Solely relying on dependency injection does not resolve this as it decouples token storage from
        /// authenticator from an outside perspective. Utilizing this property ensures, the same instance is used
        /// by the authentication consumer and its producer.</remarks>
        /// <returns>The token database model provider that is used to store the authentication data.</returns>
        IAuthenticationDataProvider<T> GetAuthenticationDataProvider();
    }
}
