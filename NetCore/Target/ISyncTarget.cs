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
using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncTarget<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncAsset : BaseSyncAsset<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseTerm : ISyncLicenseTerm
        where TSyncReleaseDetails : ISyncReleaseDetails
        where TSyncDownloadConstraints : ISyncDownloadConstraints
    {
        /// <summary>
        /// Provides information about features this sync target supports and thus is capable of.
        /// </summary>
        /// <returns>Must not return <c>null</c> but always a valid instance!</returns>
        BaseSyncTargetCapabilities GetCapabilities();

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

        #region GenericMetaData

        /// <summary>
        /// Is called right before the generic meta data is about to be synced but after <see cref="BeforeSyncAsync"/>.
        /// </summary>
        ///
        /// <remarks>This hook is only called in case the generic meta data is to be synchronized. Usually this is
        /// done on a regular and scheduled basis. Additionally to a re-scheduled background task for synchronisation
        /// asset content is synced on demand - without any generic meta data.
        ///
        /// <para>Although this task is executed in an asynchronous way, synchronisation is only started after this
        /// tasks ends successfully. In case the tasks returns <c>false</c> synchronisation of generic meta data is
        /// aborted immediately. In case the task crashes with an exception, overall synchronisation is terminated
        /// without syncing any asset.</para></remarks>
        /// <returns>The task returns a boolean value, indicating whether to continue meta data synchronisation or not.
        /// <c>true</c> means, synchronisation will continue, whereas <c>false</c> aborts the sync task without further
        /// notice and without calling <see cref="AfterGenericMetadataSyncAsync"/>.
        /// </returns>
        /// <exception cref="Exception">Any such exception indicates a faulty ending, immediately aborting the
        /// overall synchronization. So make sure all exceptions to be ignored are caught within the task.</exception>
        Task<bool> BeforeGenericMetadataSyncAsync();

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

        /// <summary>
        /// After the synchronisation of all generic meta data this is called, before any syncing of assets with
        /// <see cref="ImportAssetsAsync"/>.
        /// </summary>
        ///
        /// <remarks>This hook is only called in case the generic meta data has been synchronized without any exception.
        ///
        /// <para>Although this task is executed in an asynchronous way, synchronisation of assets only starts after
        /// this tasks ends successfully. Any aborts, crashes or unsuccessfully endings are aborting the overall
        /// synchronisation process immediately - without syncing assets. Such ending usually is indicated by throwing
        /// an exception.
        /// </para>
        /// </remarks>
        /// <exception cref="Exception">Any such exception indicates a faulty ending, immediately aborting
        /// synchronization of assets. So make sure all exceptions to be ignored are caught within the task.</exception>
        Task AfterGenericMetadataSyncAsync();

        #endregion

        #region Assets

        /// <summary>
        /// Is called right before assets are about to be synced but after <see cref="BeforeSyncAsync"/> and after
        /// <see cref="AfterGenericMetadataSyncAsync"/>.
        /// </summary>
        ///
        /// <remarks>This hook is only called right before any assets are to be synced.</remarks>
        /// <returns>The task returns a boolean value, indicating whether to continue asset synchronisation or not.
        /// <c>true</c> means, synchronisation will continue, whereas <c>false</c> aborts the sync task without further
        /// notice and without calling <see cref="AfterAssetsSyncAsync"/> or <see cref="AfterSyncAsync"/>.</returns>
        /// <exception cref="Exception">Any such exception indicates a faulty ending, immediately aborting
        /// synchronization. Hence <see cref="AfterAssetsSyncAsync"/> and <see cref="AfterSyncAsync"/></exception> are
        /// not executed.
        Task<bool> BeforeAssetsSyncAsync();

        Task<string> GetTargetCompoundAssetUuidAsync(string smintIoAssetUuid, string recommendedFileName);
        Task<string> GetTargetAssetBinaryUuidAsync(string smintIoAssetUuid, string smintIoBinaryUuid, string recommendedFileName);

        Task ImportNewTargetAssetsAsync(IList<TSyncAsset> newTargetAssets);
        Task UpdateTargetAssetsAsync(IList<TSyncAsset> updatedTargetAssets);

        Task ImportNewTargetCompoundAssetsAsync(IList<TSyncAsset> newTargetCompoundAssets);
        Task UpdateTargetCompoundAssetsAsync(IList<TSyncAsset> updatedTargetCompoundAssets);

        /// <summary>
        /// After the synchronisation of all assets with <see cref="ImportAssetsAsync"/> this is called, but before
        /// <see cref="AfterSyncAsync"/>.
        /// </summary>
        ///
        /// <exception cref="Exception">Any exception indicates a faulty ending, immediately aborting
        /// synchronization of assets without calling <see cref="AfterSyncAsync"/>.
        /// So make sure all exceptions to be ignored are caught within the task.</exception>
        Task AfterAssetsSyncAsync();

        #endregion

        Task HandleAuthenticatorExceptionAsync(AuthenticatorException exception);
        Task HandleSyncJobExceptionAsync(SyncJobException exception);

        /// <summary>
        /// A hook to be called after all synchronisation took place.
        /// </summary>
        ///
        /// <remarks>After all synchronisation took place, some clean-up might be necessary). The task returned by this hook is 
        /// executed after all synchronisation has ended - no matter whether successfully or not.
        /// </remarks>
        /// <exception cref="Exception">Any such exception is being caught and ignored. The task does not need to take
        /// special care.</exception>
        Task AfterSyncAsync();

        /// <summary>
        /// A hook that will be called when the target asset has been created, but not yet filled.
        /// </summary>
        /// <param name="targetAsset">The prepared target asset</param>
        void PrepareTargetAsset(TSyncAsset targetAsset);
    }
}


