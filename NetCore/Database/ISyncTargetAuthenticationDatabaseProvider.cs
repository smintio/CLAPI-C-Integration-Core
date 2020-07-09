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

using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database
{
    /// <summary>
    /// Implementors store authentication data for remote integration targets.
    ///
    /// <remarks>Remote integration targets might use various type of authentication schemes and authentication
    /// data. This data need to be stored, which is done by this interface. It looks similar to the
    /// <see cref="ITokenDatabaseProvider"/> , which it is. Nonetheless, since .NET does not support named
    /// type dependency injection, a new class is required to distinguish between Smint.io authentication data
    /// and remote target authentication data.</remarks>
    /// </summary>
    public interface ISyncTargetAuthenticationDatabaseProvider<T> : IAuthenticationDatabaseProvider<T>
    {
    }
}
