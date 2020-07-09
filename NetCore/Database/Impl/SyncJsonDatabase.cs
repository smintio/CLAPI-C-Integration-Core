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
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Impl
{
    /// <summary>
    /// Stores the sync database into a JSON file named "sync_database.json" in the current working directory of the
    /// process.
    /// </summary>
    public class SyncJsonDatabase : JsonFileDatabase<SyncDatabaseModel>, ISyncDatabaseProvider
    {
        public SyncJsonDatabase() : base("sync_database.json") {}

        public async Task<SyncDatabaseModel> GetSyncDatabaseModelAsync()
        {
            return await LoadDataAsync().ConfigureAwait(false);
        }

        public async Task SetSyncDatabaseModelAsync(SyncDatabaseModel syncDatabaseModel)
        {
            await this.StoreDataAsync(syncDatabaseModel).ConfigureAwait(false);
        }
    }
}
