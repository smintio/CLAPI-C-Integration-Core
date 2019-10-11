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

using System.Collections.Generic;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncTarget
    {
        /// <summary>
        /// Provides information about features this sync target supports and thus is capable of.
        /// </summary>
        /// <returns>Must not return <c>null</c> but always a valid instance!</returns>
        ISyncTargetCapabilities GetCapabilities();

        /// <summary>
        /// A hook to be called before any synchronisation is started in this turn.
        /// </summary>
        ///
        /// <remarks>This hook is called right before any synchronisation takes place. If some preparations or are
        /// to be made in advance, a task must be returned. If synchronisation does not need to be prepared or any
        /// status checked or altered, then simply return an empty task, doing nothing.
        ///
        /// <para>Although this task is executed in an asynchronous way,
        /// synchronisation is only started after this tasks ends successfully. In case the tasks aborts, crashes or
        /// ends unsuccessfully, synchronisation is terminated immediately with proper logging. Such ending usually is
        /// indicated by throwing an exception.
        /// </para>
        /// </remarks>
        /// <returns>The task returns a boolean value, indicating whether to continue synchronisation or not.
        /// <c>true</c> means, synchronisation will continue, whereas <c>false</c> aborts the sync task without further
        /// notice.</returns>
        /// <exception cref="Exception">Any such exception indicates a faulty ending, immediately aborting
        /// synchronization. So make sure all exceptions to be ignored are caught within the task.</exception>
        Task<bool> BeforeSyncAsync();

        Task ImportContentProvidersAsync(IList<SmintIoMetadataElement> contentProviders);

        Task ImportContentTypesAsync(IList<SmintIoMetadataElement> contentTypes);
        Task ImportBinaryTypesAsync(IList<SmintIoMetadataElement> binaryTypes);

        Task ImportContentCategoriesAsync(IList<SmintIoMetadataElement> contentCategories);

        Task ImportLicenseTypesAsync(IList<SmintIoMetadataElement> licenseTypes);

        Task ImportReleaseStatesAsync(IList<SmintIoMetadataElement> releaseStates);

        Task ImportLicenseExclusivitiesAsync(IList<SmintIoMetadataElement> licenseExclusivities);
        Task ImportLicenseUsagesAsync(IList<SmintIoMetadataElement> licenseUsages);
        Task ImportLicenseSizesAsync(IList<SmintIoMetadataElement> licenseSizes);
        Task ImportLicensePlacementsAsync(IList<SmintIoMetadataElement> licensePlacements);
        Task ImportLicenseDistributionsAsync(IList<SmintIoMetadataElement> licenseDistributions);
        Task ImportLicenseGeographiesAsync(IList<SmintIoMetadataElement> licenseGeographies);
        Task ImportLicenseIndustriesAsync(IList<SmintIoMetadataElement> licenseIndustries);
        Task ImportLicenseLanguagesAsync(IList<SmintIoMetadataElement> licenseLanguages);
        Task ImportLicenseUsageLimitsAsync(IList<SmintIoMetadataElement> licenseUsageLimits);

        Task ImportAssetsAsync(string folderName, IList<SmintIoAsset> assets);

        Task HandleAuthenticatorExceptionAsync(SmintIoAuthenticatorException exception);
        Task HandleSyncJobExceptionAsync(SmintIoSyncJobException exception);

        /// <summary>
        /// A hook to be called after all synchronisation took place.
        /// </summary>
        ///
        /// <remarks>After all synchronisation took place, some clean-up might be necessary, besides <c>ClearGenericMetadataCaches</c>
        /// (see <see cref="ClearGenericMetadataCaches"/>). The task returned by this hook is executed after all synchronisation has
        /// ended - no matter whether successfully or not - but <i>before</i> <see cref="ClearGenericMetadataCaches"/> is called. 
        /// So the running task still has access to all the created caches.
        /// </remarks>
        /// <exception cref="Exception">Any such exception is being caught and ignored. The task does not need to take
        /// special care.</exception>
        Task AfterSyncAsync();

        /// <summary>
        /// Clears all cached generic meta data.
        /// </summary>
        ///
        /// <remarks>During synchronisation of license information, asset types, release states and content categories,
        /// a lot of meta data is usually cached locally to avoid multiple remote lookups. This speeds-up the
        /// synchronisation of these meta data items. But before synchronisation of content takes place and at the end
        /// of each synchronisation process, this cache is cleared. In the former case it will guarantee a fresh start
        /// and will avoid any errors due to failure in meta data sync process. The latter releases all memory
        /// consumed by the cache.
        /// </remarks>
        void ClearGenericMetadataCaches();
    }
}
