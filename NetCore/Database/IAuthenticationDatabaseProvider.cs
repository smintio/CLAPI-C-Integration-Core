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

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database
{
    /// <summary>
    /// A common interface for authentication data providers.
    /// </summary>
    ///
    /// <remarks>Authentication data is used with the CLAPI-C platform and possibly with remote target systems.
    /// Both need different authentication data. Whereas the CLAPI-C platform is well known, remote target systems
    /// are too diverse to set a specific type of authentication data. Hence a generic type is used.</remarks>
    /// <typeparam name="T">The type of data the authentication data provider will provide.</typeparam>
    public interface IAuthenticationDatabaseProvider<T>
    {
        /// <summary>
        /// Returns the authentication data read from some storage system or maybe created on-the-fly.
        /// </summary>
        /// <returns>Authentication data or <c>null</c> if none is available.</returns>
        Task<T> GetAuthenticationDatabaseModelAsync();

        /// <summary>
        /// Save authentication data or even store it to some storage system.
        /// </summary>
        /// <param name="authenticationData">The authentication data to save or store.</param>
        /// <returns></returns>
        Task SetAuthenticationDatabaseModelAsync(T authenticationData);
    }
}
