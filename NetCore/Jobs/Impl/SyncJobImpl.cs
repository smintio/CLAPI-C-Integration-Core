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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Target;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl
{
    internal class SyncJobImpl: ISyncJob
    {
        private const string Folder = "temp";

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;
        private readonly ISyncDatabaseProvider _syncDatabaseProvider;

        private readonly ISmintIoApiClientProvider _smintIoClient;

        private readonly ISyncTarget _syncTarget;

        private readonly ILogger _logger;

        public SyncJobImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            ITokenDatabaseProvider tokenDatabaseProvider,
            ISyncDatabaseProvider syncDatabaseProvider,            
            ISmintIoApiClientProvider smintIoClient,
            ISyncTarget syncTarget,
            ILogger<SyncJobImpl> logger)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _tokenDatabaseProvider = tokenDatabaseProvider;
            _syncDatabaseProvider = syncDatabaseProvider;

            _smintIoClient = smintIoClient;

            _syncTarget = syncTarget;

            _logger = logger;
        }

        public async Task SynchronizeAsync(bool synchronizeGenericMetadata)
        {
            await Semaphore.WaitAsync();

            try
            {
                var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();
                settingsDatabaseModel.ValidateForSync();

                if (!_syncTarget.GetCapabilities().IsMultiLanguageSupported() &&
                    settingsDatabaseModel.ImportLanguages.Length > 1)
                {
                    throw new SmintIoSyncJobException(
                        SmintIoSyncJobException.SyncJobError.Generic,
                        "SyncTarget supports only one language but multiple language are set to be synced!"
                    );
                }

                var tokenDatabaseModel = await _tokenDatabaseProvider.GetTokenDatabaseModelAsync();
                tokenDatabaseModel.ValidateForSync();

                var cancelTask = !await _syncTarget.BeforeSyncAsync();
                
                if (cancelTask)
                {
                    _logger.LogInformation("'BeforeSync' task terminated with 'false', indicating to abort sync.");

                    return;
                }

                if (synchronizeGenericMetadata)
                {
                    await SynchronizeGenericMetadataAsync();
                }
                
                await SynchronizeAssetsAsync();

                await _syncTarget.AfterSyncAsync();
            }
            catch (SmintIoAuthenticatorException e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleAuthenticatorExceptionAsync(e);
            }
            catch (SmintIoSyncJobException e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleSyncJobExceptionAsync(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleSyncJobExceptionAsync(new SmintIoSyncJobException(SmintIoSyncJobException.SyncJobError.Generic, e.Message));
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async Task SynchronizeGenericMetadataAsync()
        {
            _logger.LogInformation("Starting Smint.io generic metadata synchronization...");

            var genericMetadata = await _smintIoClient.GetGenericMetadataAsync();

            await _syncTarget.ImportContentProvidersAsync(genericMetadata.ContentProviders);

            await _syncTarget.ImportContentTypesAsync(genericMetadata.ContentTypes);
            await _syncTarget.ImportBinaryTypesAsync(genericMetadata.BinaryTypes);

            await _syncTarget.ImportContentCategoriesAsync(genericMetadata.ContentCategories);

            await _syncTarget.ImportLicenseTypesAsync(genericMetadata.LicenseTypes);

            await _syncTarget.ImportReleaseStatesAsync(genericMetadata.ReleaseStates);

            await _syncTarget.ImportLicenseExclusivitiesAsync(genericMetadata.LicenseExclusivities);
            await _syncTarget.ImportLicenseUsagesAsync(genericMetadata.LicenseUsages);
            await _syncTarget.ImportLicenseSizesAsync(genericMetadata.LicenseSizes);
            await _syncTarget.ImportLicensePlacementsAsync(genericMetadata.LicensePlacements);
            await _syncTarget.ImportLicenseDistributionsAsync(genericMetadata.LicenseDistributions);
            await _syncTarget.ImportLicenseGeographiesAsync(genericMetadata.LicenseGeographies);
            await _syncTarget.ImportLicenseIndustriesAsync(genericMetadata.LicenseIndustries);
            await _syncTarget.ImportLicenseLanguagesAsync(genericMetadata.LicenseLanguages);
            await _syncTarget.ImportLicenseUsageLimitsAsync(genericMetadata.LicenseUsageLimits);

            _syncTarget.ClearGenericMetadataCaches();

            _logger.LogInformation("Finished Smint.io generic metadata synchronization");
        }

        private async Task SynchronizeAssetsAsync()
        {
            _logger.LogInformation("Starting Smint.io asset synchronization...");

            var folderName = Folder + new Random().Next(1000000, 9999999);

            var syncDatabaseModel = await _syncDatabaseProvider.GetSyncDatabaseModelAsync();

            // get last committed state

            string continuationUuid = syncDatabaseModel?.ContinuationUuid;

            try
            {
                IList<SmintIoAsset> assets = null;

                do
                {
                    (assets, continuationUuid) = await _smintIoClient.GetAssetsAsync(continuationUuid);

                    if (assets != null && assets.Any())
                    {
                        CreateTempFolder(folderName);

                        await _syncTarget.ImportAssetsAsync(folderName, assets);

                        // store committed data

                        await _syncDatabaseProvider.SetSyncDatabaseModelAsync(new SyncDatabaseModel()
                        {
                            ContinuationUuid = continuationUuid
                        });
                    }

                    _logger.LogInformation($"Synchronized {assets.Count()} Smint.io assets");
                } while (assets != null && assets.Any());

                _logger.LogInformation("Finished Smint.io asset synchronization");

                _syncTarget.ClearGenericMetadataCaches();
            }
            finally
            {
                RemoveTempFolder(folderName);
            }
        }

        private void CreateTempFolder(string folderName)
        {
            Directory.CreateDirectory(folderName);
        }

        private void RemoveTempFolder(string folderName)
        {
            if (Directory.Exists(folderName))
                Directory.Delete(folderName, true);
        }
    }
}
