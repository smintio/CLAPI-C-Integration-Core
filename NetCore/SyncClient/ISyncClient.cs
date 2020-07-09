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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.SyncClient
{
    /// <summary>
    /// Represents a tenant based synchronization client, that will be used to start a synchronization.
    /// </summary>
    ///
    /// <remarks>Synchronization clients perform long running tasks. They may schedule execution on their own by
    /// registering itself with some messaging service or timer service. On disposing the sync client, all those
    /// registrations must be undone, though.
    /// </remarks>
    public interface ISyncClient
    {
        /// <summary>
        /// Reflects the current state of the synchronisation client. It must be set to <c>false</c> if
        /// <see cref="IsErrorState"/> is <c>true</c>.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// If <c>true</c> then an unrecoverable error occurred and <see cref="IsRunning"/> will be <c>false</c>.
        /// </summary>
        public bool IsErrorState { get; }

        /// <summary>
        /// Provides all error messages of the last run if <see cref="IsErrorState"/> is <c>true</c>.
        /// </summary>
        /// <remarks>The list or error messages is cleared, as soon as <see cref="StartAsync"/> is called, as this list
        /// only contains the messages of the last run.
        /// </remarks>
        public List<string> ErrorMessages { get; }

        /// <summary>
        /// Start the synchronisation task and register with any scheduler or messaging service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop the synchronisation process and unregisters from any scheduler or messaging service.
        /// </summary>
        ///
        /// <remarks>Resources are not yet disposed but execution of the sync task is stopped and all scheduling is
        /// undone.
        /// </remarks>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Immediately queues an execution of a synchronisation process.
        /// </summary>
        ///
        /// <remarks>Multiple Synchronisation between the same pair (source + target) may not run in parallel. Hence
        /// a queue is used to queue more synchronisation tasks. If another sync task is already waiting for execution,
        /// no more tasks will be added to the queue as this is redundant.
        /// </remarks>
        public Task TriggerSyncAsync(bool syncWithMetaData);
    }
}