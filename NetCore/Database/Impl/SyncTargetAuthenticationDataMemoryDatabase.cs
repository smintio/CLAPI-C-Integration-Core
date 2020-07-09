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

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Impl
{
    /// <summary>
    /// A dummy default memory only implementation of the remote authentication data database.
    /// <remarks>This is ephemeral, memory only database. In case the sync target is kept in memory, it is sufficient.
    /// However, if the sync target client is released frequently, then it will not suffice, as the authentication
    /// would be needed to re-created for every run. In such situations, please consider to store the authentication
    /// data in some database or in file system.</remarks>
    /// </summary>
    /// <typeparam name="T">Is passed to <see cref="IRemoteAuthDatabaseProvider{T}"/> and defines the type of the
    /// authentication data that is used with the authenticator.</typeparam>
    public class SyncTargetAuthenticationDataMemoryDatabase<T> : ISyncTargetAuthenticationDatabaseProvider<T> where T : class, new()
    {
        private T _syncTargetAuthenticationDatabaseModel;

        public SyncTargetAuthenticationDataMemoryDatabase()
        {
            _syncTargetAuthenticationDatabaseModel = new T();
        }

        public Task<T> GetAuthenticationDatabaseModelAsync()
        {
            return Task.FromResult(_syncTargetAuthenticationDatabaseModel);
        }

        public Task SetAuthenticationDatabaseModelAsync(T syncTargetAuthenticationDatabaseModel)
        {
            _syncTargetAuthenticationDatabaseModel = syncTargetAuthenticationDatabaseModel;
            return Task.FromResult<dynamic>(null);
        }
    }
}
