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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Services
{
    internal class TimedSynchronizerService : IHostedService, IDisposable
    {
        private readonly ISyncJob _syncJob;

        private Timer _timer;

        private readonly ILogger _logger;

        public TimedSynchronizerService(
            ISyncJob syncJob,
            ILogger<TimedSynchronizerService> logger)
        {
            _syncJob = syncJob;

            _logger = logger;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting timed synchronizer service...");

            _timer = new Timer(DoWorkAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Started timed synchronizer service");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping timed synchronizer service...");

            _timer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation("Stopped timed synchronizer service");

            return Task.CompletedTask;
        }

        private async void DoWorkAsync(object state)
        {
            // sync generic metadata, because this is our regular, lazy job

            await _syncJob.SynchronizeAsync(synchronizeGenericMetadata: true);
        }
    }
}
