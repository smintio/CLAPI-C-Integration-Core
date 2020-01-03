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

using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Services;
using SmintIo.CLAPI.Consumer.Integration.Core.Target;
using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IntegrationCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddSmintIoClapicIntegrationCore<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>(this IServiceCollection services)
            where TSyncAsset : BaseSyncAsset<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
            where TSyncLicenseTerm : ISyncLicenseTerm
            where TSyncReleaseDetails : ISyncReleaseDetails
            where TSyncDownloadConstraints : ISyncDownloadConstraints
        {            
            Console.WriteLine("Initializing CLAPI-C Integration Core...");

            services.AddSingleton<ISyncJob, SyncJobImpl<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>>();

            services.AddSingleton<ISyncJobExecutionQueue, DefaultSyncJobExecutionQueue>();
            services.AddSingleton<ISmintIoApiClientProvider, SmintIoApiClientProviderImpl>();            

            services.AddHostedService<TimedSynchronizerService>();
            services.AddHostedService<PusherService>();

            services.AddSingleton<ISmintIoAuthenticator, SmintIoAuthenticatorImpl>();

            Console.WriteLine("CLAPI-C Integration Core initialized successfully");

            return services;
        }
    }
}
