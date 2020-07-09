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

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Models
{
    /// <summary>
    /// Authentication data of remote targets to integrate with and sync to.
    /// <remarks>Since the needs of authentication data for remote target systems is very diverse, it is best each
    /// target integration handles the structure itself. In order to support any kind of data, the authentication data
    /// is stored as string value. Target integration implementation will need to take care to serialize or deserialize
    /// the data to and from JSON, if more complex data structures are needed.</remarks>
    /// </summary>
    public class SyncTargetAuthenticationDatabaseModel
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public string AuthData { get; set; }

        public DateTimeOffset? Expiration { get; set; }
    }
}
