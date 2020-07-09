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
using Autofac;
using Client.Options;
using Microsoft.Extensions.Configuration;
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
    public interface ISyncClientFactory
    {
        /// <summary>
        /// Creates a new tenant based client synchronisation instance.
        /// </summary>
        ///
        /// <remarks>The provided data is used to configure the synchronisation client, that can be used to invoke
        /// the synchronisation process immediately and after some time.
        /// </remarks>
        /// <param name="scope">A Scoped service provider to deliver configuration and other instances
        /// to the sync client. The scope has been configured previously with <see cref="ConfigureContainerBuilder"/>.
        /// Proper instances of <see cref="SmintIoAppOptions"/>, <see cref="SmintIoAuthOptions"/>,
        /// </param>
        /// <returns>A worker task to synchronize tenant's asset with a target DAM.</returns>
        ISyncClient CreateSyncClient(ILifetimeScope scope) => scope.Resolve<ISyncClient>();

        /// <summary>
        /// Configures the Autofac dependency injector to contain all necessary dependencies to create a sync client.
        /// </summary>
        ///
        /// <remarks>To separate sync clients from each other, a lifetime scope of Autofac is used. This lifetime
        /// scope needs to be configured to contain all necessary dependency definitions, prior to creating the
        /// sync client. It will be pre-configured with necessary references to <see cref="SmintIoAppOptions"/>,
        /// <see cref="SmintIoAuthOptions"/>, and <see cref="IConfiguration"/>.
        /// See <see href="https://autofac.org/apidoc/html/7021277C.htm">Autofac API documentation</see>.
        /// </remarks>
        /// <param name="containerBuilder">The container builder that can be used to configure the DI container.</param>
        /// <returns>A worker task to synchronize tenant's asset with a target DAM.</returns>
        ISyncClientFactory ConfigureContainerBuilder(ContainerBuilder containerBuilder);


        /// <summary>
        /// The name of the synchronisation target that is covered by the implementation of the interface.
        /// </summary>
        ///
        /// <remarks>The name must be unique as it will be used with some sort of mapping or service locator.</remarks>
        public string SyncTargetName { get; }
    }
}