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
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Client.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Services;
using SmintIo.CLAPI.Consumer.Integration.Core.SyncClient;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Factory
{
    /// <summary>
    /// Implementing classes create new synchronisation clients of specific types that can be used to sync the tenant's
    /// asset data to a target DAM.
    /// </summary>
    ///
    /// <remarks>Synchronisation clients need to be created with a bunch of configuration and may be their own
    /// dependency injection stack. Factory instances take all the necessary data, create sync tailored for the specific
    /// client and will pass them to the caller to execute these in a scheduled way.
    /// </remarks>
    ///
    /// <remarks>For each supported DAM vendor, a specific implementation of this interface must be provided to
    /// setup the synchronisation client accordingly. Each instance is used as a singleton with some mapping tool or
    /// service locator pattern!
    /// </remarks>
    /// <typeparam name="T">The type of the <see cref="ISyncClient"/>, which is used to implement
    /// <see cref="SyncTargetName"/>.</typeparam>
    public class BaseSyncClientFactoryImpl<T> : ISyncClientFactory where T : ISyncClient
    {
        public IConfiguration Configuration { get; set; }
        
        public BaseSyncClientFactoryImpl(IConfiguration appConfig)
        {
            Configuration = appConfig;
        }

        public virtual ISyncClientFactory ConfigureContainerBuilder(ContainerBuilder containerBuilder)
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITimedSynchronizerService, TimedSynchronizerService>();
            services.AddSingleton<IPusherService, PusherService>();

            services.AddTransient<IOAuthAuthenticationRefresher, OAuthAuthenticationRefresherImpl>();
            services.AddTransient<IOAuthAuthenticator, OAuthAuthenticatorImpl>();

            containerBuilder.Populate(services);

            return this;
        }

        /// <summary>
        /// The name of the synchronisation target that is covered by the implementation of the interface.
        /// </summary>
        ///
        /// <remarks>The name is derived from the class name by removing "sync", "client" and "impl"
        /// from the lower case name of <c>T</c>.</remarks>
        public string SyncTargetName { get; } = nameof(T)
            .ToLower()
            .Replace("sync", "")
            .Replace("client", "")
            .Replace("impl", "")
        ;
    }
}