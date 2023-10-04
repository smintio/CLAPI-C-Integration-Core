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
using PusherClient;
using System.Threading;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using System.Net.Http.Headers;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Services
{
    internal class PusherService : IPusherService, IHostedService
    {
        private readonly ISmintIoSettingsDatabaseProvider _smintIoSettingsDatabaseProvider;
        private readonly ISmintIoTokenDatabaseProvider _smintIoTokenDatabaseProvider;

        private Pusher _pusher;

        private Channel _channel;

        private readonly ISyncJobExecutionQueue _jobExecutionQueue;
        private readonly ISyncJob _syncJob;

        private readonly ILogger _logger;

        public PusherService(
            ISmintIoSettingsDatabaseProvider smintIoSettingsDatabaseProvider,
            ISmintIoTokenDatabaseProvider smintIoTokenDatabaseProvider,            
            ISyncJobExecutionQueue jobExecutionQueue,
            ISyncJob syncJob,
            ILogger<PusherService> logger)
        {
            _smintIoSettingsDatabaseProvider = smintIoSettingsDatabaseProvider;
            _smintIoTokenDatabaseProvider = smintIoTokenDatabaseProvider;

            _jobExecutionQueue = jobExecutionQueue;
            _syncJob = syncJob;

            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Pusher service...");

            await StartPusherAsync();

            _logger.LogInformation("Pusher service started");
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public async Task StopAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            _logger.LogInformation("Stopping Pusher service...");

            StopPusher();

            _logger.LogInformation("Pusher service stopped");
        }

        private async Task StartPusherAsync()
        {
            var smintIoSettingsDatabaseModel = await _smintIoSettingsDatabaseProvider.GetSmintIoSettingsDatabaseModelAsync();

            smintIoSettingsDatabaseModel.ValidateForPusher();

            var pusherAuthEndpoint = $"https://{smintIoSettingsDatabaseModel.TenantId}.clapi.smint.io/consumer/v1/notifications/pusher/auth";

            var tokenDatabaseModel = await _smintIoTokenDatabaseProvider.GetTokenDatabaseModelAsync();

            tokenDatabaseModel.ValidateForPusher();

            var authorizer = new HttpAuthorizer(pusherAuthEndpoint)
            {
                AuthenticationHeader = new AuthenticationHeaderValue("Bearer", tokenDatabaseModel.AccessToken)
            };

            _pusher = new Pusher("32f31c26a83e09dc401b", new PusherOptions()
            {
                Cluster = "eu",
                Authorizer = authorizer
            });

            _pusher.ConnectionStateChanged += ConnectionStateChanged;
            _pusher.Error += PusherError;

            await _pusher.ConnectAsync();

            if (_pusher.State == ConnectionState.Connected)
            {
                await SubscribeToPusherChannelAsync((int)smintIoSettingsDatabaseModel.ChannelId);
            }
        }

        private void StopPusher()
        {
            _channel?.UnbindAll();
            _channel?.Unsubscribe();

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            _pusher.DisconnectAsync().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            _pusher.ConnectionStateChanged -= ConnectionStateChanged;
            _pusher.Error -= PusherError;
        }

        private async Task SubscribeToPusherChannelAsync(int channelId)
        {
            _channel = await _pusher.SubscribeAsync($"private-2-{channelId}");

            _channel.Bind("global-transaction-history-update", (payload) =>
            {
                _logger.LogInformation("Received Pusher event");

                // do not sync generic metadata, because we need to be fast here

                _jobExecutionQueue.AddJobForPushEvent(async () =>
                    await _syncJob.SynchronizeAsync(synchronizeGenericMetadata: false)
                );
            });
        }

        private void ConnectionStateChanged(object sender, ConnectionState state)
        {
            _logger.LogInformation($"Pusher connection state changed to {state}");
        }

        private void PusherError(object sender, PusherException pusherException)
        {
            _logger.LogError(pusherException, "An Pusher exception occured");
        }
    }
}
