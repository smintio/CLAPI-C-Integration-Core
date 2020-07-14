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
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;
using SmintIo.CLAPI.Consumer.Integration.Core.SyncClient;
using SmintIo.CLAPI.Consumer.Integration.Core.Target;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Services
{
    public class SingleTargetSyncService : IHostedService, IDisposable
    {
        private readonly ISyncClientFactory _syncClientFactory;
        private readonly ILifetimeScope _dependencyScope;
        private readonly ILogger<SingleTargetSyncService> _logger;

        private ISyncClient _syncClient;
        private ILifetimeScope _clientScope;

        public SingleTargetSyncService(
            ISyncClientFactory syncClientFactory,
            ILifetimeScope dependencyScope,
            ILogger<SingleTargetSyncService> logger)
        {
            _syncClientFactory = syncClientFactory;
            _dependencyScope = dependencyScope;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_syncClientFactory is IDisposable disposableSyncClientFactory)
            {
                disposableSyncClientFactory.Dispose();
            }

            if (_syncClient is IDisposable disposableSyncClient)
            {
                disposableSyncClient.Dispose();
            }
            _clientScope?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting synchronizer service...");

            if (_syncClient == null && _clientScope == null)
            {
                _clientScope = _dependencyScope.BeginLifetimeScope(
                     containerConfig => _syncClientFactory.ConfigureContainerBuilder(containerConfig));

                // The scope is disposed right after creation of the sync client. This should not be a problem as
                // the client holds references to all required instances after creation.
                _syncClient = _syncClientFactory.CreateSyncClient(_clientScope);

                var authenticator = _clientScope.Resolve<ISmintIoAuthenticator>();
                Debug.Assert(authenticator != null, nameof(authenticator) + " != null");

                // we have a system browser based authenticator here, which will work synchronously
                await authenticator.InitializeAuthenticationAsync();
            }

            await _syncClient.StartAsync(cancellationToken);

            _logger.LogInformation("Started synchronizer service");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_syncClient != null)
            {
                _logger.LogInformation("Stopping timed synchronizer service...");
                await _syncClient.StopAsync(cancellationToken);
                _clientScope.Dispose();
                _clientScope = null;
                _logger.LogInformation("Stopped synchronizer service");
            }
            else
            {
                _logger.LogInformation("Synchronizer service has not been started yet");
            }
        }
    }
}
