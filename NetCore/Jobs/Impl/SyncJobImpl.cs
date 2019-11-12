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
using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl
{
    internal class SyncJobImpl<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> : ISyncJob
        where TSyncAsset : SyncAssetImpl<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseOption : SyncLicenseOptionImpl
        where TSyncLicenseTerm : SyncLicenseTermImpl
        where TSyncReleaseDetails : SyncReleaseDetailsImpl
        where TSyncDownloadConstraints : SyncDownloadConstraintsImpl
    {
        private const string Folder = "temp";

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;
        private readonly ISyncDatabaseProvider _syncDatabaseProvider;

        private readonly ISmintIoApiClientProvider _smintIoClient;

        private readonly ISyncTarget<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> _syncTarget;

        private readonly ILogger _logger;

        public SyncJobImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            ITokenDatabaseProvider tokenDatabaseProvider,
            ISyncDatabaseProvider syncDatabaseProvider,            
            ISmintIoApiClientProvider smintIoClient,
            ISyncTarget<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> syncTarget,
            ILogger<SyncJobImpl<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>> logger)
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

            var cancelMetadataSync = !await _syncTarget.BeforeGenericMetadataSyncAsync();
            if (cancelMetadataSync)
            {
                _logger.LogInformation("'BeforeGenericMetadataSyncAsync' task aborted meta data sync");

                return;
            }

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

            await _syncTarget.AfterGenericMetadataSyncAsync();

            _syncTarget.ClearGenericMetadataCaches();

            _logger.LogInformation("Finished Smint.io generic metadata synchronization");
        }

        private async Task SynchronizeAssetsAsync()
        {
            _logger.LogInformation("Starting Smint.io asset synchronization...");

            var cancelAssetsSync = !await _syncTarget.BeforeAssetsSyncAsync();
            if (cancelAssetsSync)
            {
                _logger.LogInformation("'BeforeAssetsSyncAsync' task aborted assets sync");

                return;
            }

            var folderName = Folder + new Random().Next(1000000, 9999999);

            var syncDatabaseModel = await _syncDatabaseProvider.GetSyncDatabaseModelAsync();

            // get last committed state

            string continuationUuid = syncDatabaseModel?.ContinuationUuid;

            try
            {
                IList<SmintIoAsset> rawAssets = null;

                bool compoundAssetsSupported = _syncTarget.GetCapabilities().IsCompoundAssetsSupported();
                bool binaryUpdatesSupported = _syncTarget.GetCapabilities().IsBinaryUpdatesSupported();

                do
                {
                    (rawAssets, continuationUuid) = await _smintIoClient.GetAssetsAsync(continuationUuid, compoundAssetsSupported, binaryUpdatesSupported);

                    if (rawAssets != null && rawAssets.Any())
                    {
                        CreateTempFolder(folderName);

                        var targetAssets = await TransformAssetsAsync(rawAssets);

                        IList<TSyncAsset> newTargetAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> updatedTargetAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> newTargetCompoundAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> updatedTargetCompoundAssets = new List<TSyncAsset>();

                        foreach (var targetAsset in targetAssets)
                        {
                            string targetAssetUuid = await _syncTarget.GetTargetAssetUuidAsync(targetAsset.Uuid, targetAsset.BinaryUuid, targetAsset.IsCompoundAsset);

                            if (targetAsset.IsCompoundAsset)
                            {
                                if (!string.IsNullOrEmpty(targetAssetUuid))
                                {
                                    targetAsset.SetTargetAssetUuid(targetAssetUuid);

                                    updatedTargetCompoundAssets.Add(targetAsset);
                                }
                                else
                                {
                                    newTargetCompoundAssets.Add(targetAsset);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(targetAssetUuid))
                                {
                                    targetAsset.SetTargetAssetUuid(targetAssetUuid);

                                    updatedTargetAssets.Add(targetAsset);
                                }
                                else
                                {
                                    newTargetAssets.Add(targetAsset);
                                }
                            }
                        }

                        if (newTargetAssets.Count > 0)
                            await _syncTarget.CreateTargetAssetsAsync(folderName, newTargetAssets);

                        if (updatedTargetAssets.Count > 0)
                            await _syncTarget.UpdateTargetAssetsAsync(folderName, updatedTargetAssets);

                        if (newTargetCompoundAssets.Count > 0)
                            await _syncTarget.CreateTargetCompoundAssetsAsync(newTargetCompoundAssets);

                        if (updatedTargetCompoundAssets.Count > 0)
                            await _syncTarget.UpdateTargetCompoundAssetsAsync(updatedTargetCompoundAssets);
                        
                        // store committed data

                        await _syncDatabaseProvider.SetSyncDatabaseModelAsync(new SyncDatabaseModel()
                        {
                            ContinuationUuid = continuationUuid
                        });
                    }

                    _logger.LogInformation($"Synchronized {rawAssets.Count()} Smint.io assets");
                } while (rawAssets != null && rawAssets.Any());

                _logger.LogInformation("Finished Smint.io asset synchronization");

                await _syncTarget.AfterAssetsSyncAsync();

                _syncTarget.ClearGenericMetadataCaches();
            }
            finally
            {
                RemoveTempFolder(folderName);
            }
        }

        private async Task<IList<TSyncAsset>> TransformAssetsAsync(IList<SmintIoAsset> rawAssets)
        {
            IList<TSyncAsset> assets = new List<TSyncAsset>();

            foreach (var rawAsset in rawAssets)
            {
                _logger.LogInformation($"Transforming Smint.io LPT {rawAsset.LicensePurchaseTransactionUuid}...");

                var binaries = rawAsset.Binaries;

                IList<TSyncAsset> assetPartAssets = new List<TSyncAsset>();

                foreach (var binary in binaries)
                {
                    var downloadUrl = binary.DownloadUrl;
                    var recommendedFileName = binary.RecommendedFileName;

                    var targetAsset = _syncTarget.CreateSyncAsset();

                    targetAsset.SetUuid(rawAsset.LicensePurchaseTransactionUuid);

                    targetAsset.SetFindAgainFileUuid($"{rawAsset.LicensePurchaseTransactionUuid}_{binary.Uuid}");

                    targetAsset.SetMimeType(binary.MimeType);
                    targetAsset.SetRecommendedFileName(recommendedFileName);

                    targetAsset.SetDownloadUrl(downloadUrl);

                    await SetContentMetadataAsync(targetAsset, rawAsset, binary);
                    await SetLicenseMetadataAsync(targetAsset, rawAsset);

                    assetPartAssets.Add(targetAsset);

                    assets.Add(targetAsset);
                }

                if (assetPartAssets.Count > 1)
                {
                    // we have a compound asset, consisting of more than one asset part

                    var targetCompoundAsset = _syncTarget.CreateSyncAsset();

                    targetCompoundAsset.SetUuid(rawAsset.LicensePurchaseTransactionUuid);

                    targetCompoundAsset.SetAssetParts(assetPartAssets);

                    await SetContentMetadataAsync(targetCompoundAsset, rawAsset, null);
                    await SetLicenseMetadataAsync(targetCompoundAsset, rawAsset);

                    assets.Add(targetCompoundAsset);
                }

                _logger.LogInformation($"Transformed Smint.io LPT {rawAsset.LicensePurchaseTransactionUuid}");
            }

            return assets;
        }

        private async Task SetContentMetadataAsync(TSyncAsset targetAsset, SmintIoAsset rawAsset, SmintIoBinary binary)
        {
            var contentTypeString = !string.IsNullOrEmpty(binary?.ContentType) ? binary.ContentType : rawAsset.ContentType;

            targetAsset.SetContentElementUuid(rawAsset.ContentElementUuid);
            targetAsset.SetContentProvider(await _syncTarget.GetContentProviderKeyAsync(rawAsset.Provider));
            targetAsset.SetContentType(await _syncTarget.GetContentTypeKeyAsync(contentTypeString));
            targetAsset.SetContentCategory(await _syncTarget.GetContentCategoryKeyAsync(rawAsset.Category));
            targetAsset.SetSmintIoUrl(rawAsset.SmintIoUrl);
            targetAsset.SetPurchasedAt(rawAsset.PurchasedAt);
            targetAsset.SetCreatedAt(rawAsset.CreatedAt);
            targetAsset.SetCartPurchaseTransactionUuid(rawAsset.CartPurchaseTransactionUuid);
            targetAsset.SetLicensePurchaseTransactionUuid(rawAsset.LicensePurchaseTransactionUuid);
            targetAsset.SetHasBeenCancelled(rawAsset.State == Client.Generated.LicensePurchaseTransactionStateEnum.Cancelled);

            if (!string.IsNullOrEmpty(binary?.BinaryType))
                targetAsset.SetBinaryType(await _syncTarget.GetBinaryTypeKeyAsync(binary.BinaryType));

            if (rawAsset.LastUpdatedAt != null)
                targetAsset.SetLastUpdatedAt((DateTimeOffset)rawAsset.LastUpdatedAt);

            if (binary?.Name?.Count > 0)
                targetAsset.SetNameInternal(binary.Name);
            else if (rawAsset.Name?.Count > 0)
                targetAsset.SetNameInternal(rawAsset.Name);

            if (binary?.Description?.Count > 0)
                targetAsset.SetDescription(binary.Description);
            else if (rawAsset.Description?.Count > 0)
                targetAsset.SetDescription(rawAsset.Description);

            if (!string.IsNullOrEmpty(rawAsset.ProjectUuid))
                targetAsset.SetProjectUuid(rawAsset.ProjectUuid);

            if (rawAsset.ProjectName?.Count > 0)
                targetAsset.SetProjectName(rawAsset.ProjectName);

            if (!string.IsNullOrEmpty(rawAsset.CollectionUuid))
                targetAsset.SetCollectionUuid(rawAsset.CollectionUuid);

            if (rawAsset.CollectionName?.Count > 0)
                targetAsset.SetCollectionName(rawAsset.CollectionName);

            if (rawAsset.Keywords?.Count > 0)
                targetAsset.SetKeywords(rawAsset.Keywords);

            if (rawAsset.CopyrightNotices?.Count > 0)
                targetAsset.SetCopyrightNotices(rawAsset.CopyrightNotices);

            if (binary != null)
            {
                targetAsset.SetBinaryUuidInternal(binary.Uuid);

                if (binary.Usage?.Count > 0)
                    targetAsset.SetBinaryUsageInternal(binary.Usage);

                var binaryCulture = binary.Culture;
                if (!string.IsNullOrEmpty(binaryCulture))
                    targetAsset.SetBinaryCulture(binaryCulture);

                targetAsset.SetBinaryVersionInternal(binary.Version);
            }
        }

        private async Task SetLicenseMetadataAsync(TSyncAsset targetAsset, SmintIoAsset rawAsset)
        {
            targetAsset.SetLicenseType(await _syncTarget.GetLicenseTypeKeyAsync(rawAsset.LicenseType));

            targetAsset.SetLicenseeUuid(rawAsset.LicenseeUuid);
            targetAsset.SetLicenseeName(rawAsset.LicenseeName);

            if (rawAsset.LicenseText?.Count > 0)
                targetAsset.SetLicenseText(rawAsset.LicenseText);

            if (rawAsset.LicenseOptions?.Count > 0)
                targetAsset.SetLicenseOptions(GetLicenseOptions(rawAsset.LicenseOptions));

            if (rawAsset.LicenseTerms?.Count > 0)
                targetAsset.SetLicenseTerms(await GetLicenseTermsAsync(rawAsset.LicenseTerms));

            if (rawAsset.DownloadConstraints != null)
            {
                var rawDownloadConstraints = rawAsset.DownloadConstraints;

                var targetDownloadConstraints = _syncTarget.CreateSyncDownloadConstraints();

                if (rawDownloadConstraints.MaxDownloads != null)
                    targetDownloadConstraints.SetMaxDownloads((int)rawDownloadConstraints.MaxDownloads);

                if (rawDownloadConstraints.MaxUsers != null)
                    targetDownloadConstraints.SetMaxUsers((int)rawDownloadConstraints.MaxUsers);

                if (rawDownloadConstraints.MaxReuses != null)
                    targetDownloadConstraints.SetMaxReuses((int)rawDownloadConstraints.MaxReuses);

                targetAsset.SetDownloadConstraints(targetDownloadConstraints);
            }

            if (rawAsset.ReleaseDetails != null)
            {
                var rawReleaseDetails = rawAsset.ReleaseDetails;

                var targetReleaseDetails = _syncTarget.CreateSyncReleaseDetails();

                string modelReleaseState = null;
                if (!string.IsNullOrEmpty(rawReleaseDetails.ModelReleaseState))
                    modelReleaseState = await _syncTarget.GetReleaseStateKeyAsync(rawReleaseDetails.ModelReleaseState);

                string propertyReleaseState = null;
                if (!string.IsNullOrEmpty(rawReleaseDetails.PropertyReleaseState))
                    propertyReleaseState = await _syncTarget.GetReleaseStateKeyAsync(rawReleaseDetails.PropertyReleaseState);

                if (!string.IsNullOrEmpty(modelReleaseState))
                    targetReleaseDetails.SetModelReleaseState(modelReleaseState);

                if (!string.IsNullOrEmpty(propertyReleaseState))
                    targetReleaseDetails.SetPropertyReleaseState(propertyReleaseState);

                if (rawReleaseDetails.ProviderAllowedUseComment?.Count > 0)
                    targetReleaseDetails.SetProviderAllowedUseComment(rawReleaseDetails.ProviderAllowedUseComment);

                if (rawReleaseDetails.ProviderReleaseComment?.Count > 0)
                    targetReleaseDetails.SetProviderReleaseComment(rawReleaseDetails.ProviderReleaseComment);

                if (rawReleaseDetails.ProviderUsageConstraints?.Count > 0)
                    targetReleaseDetails.SetProviderUsageConstraints(rawReleaseDetails.ProviderUsageConstraints);

                targetAsset.SetReleaseDetails(targetReleaseDetails);
            }

            if (rawAsset.IsEditorialUse != null)
                targetAsset.SetIsEditorialUse((bool)rawAsset.IsEditorialUse);

            if (rawAsset.HasLicenseTerms != null)
                targetAsset.SetHasLicenseTerms((bool)rawAsset.HasLicenseTerms);
        }

        private IList<TSyncLicenseOption> GetLicenseOptions(IList<SmintIoLicenseOptions> rawLicenseOptions)
        {
            return rawLicenseOptions.Select(rawLicenseOption =>
            {
                var targetLicenseOption = _syncTarget.CreateSyncLicenseOption();

                targetLicenseOption.SetName(rawLicenseOption.OptionName);

                if (rawLicenseOption.LicenseText?.Count > 0)
                    targetLicenseOption.SetLicenseText(rawLicenseOption.LicenseText);

                return targetLicenseOption;
            }).ToList();
        }

        private async Task<IList<TSyncLicenseTerm>> GetLicenseTermsAsync(IList<SmintIoLicenseTerm> rawLicenseTerms)
        {
            var targetLicenseTerms = new List<TSyncLicenseTerm>();

            foreach (var rawLicenseTerm in rawLicenseTerms)
            {
                var exclusivities = await GetLicenseExclusivitiesKeysAsync(rawLicenseTerm.Exclusivities);

                var allowedUsages = await GetLicenseUsagesKeysAsync(rawLicenseTerm.AllowedUsages);
                var restrictedUsages = await GetLicenseUsagesKeysAsync(rawLicenseTerm.RestrictedUsages);

                var allowedSizes = await GetLicenseSizesKeysAsync(rawLicenseTerm.AllowedSizes);
                var restrictedSizes = await GetLicenseSizesKeysAsync(rawLicenseTerm.RestrictedSizes);

                var allowedPlacements = await GetLicensePlacementsKeysAsync(rawLicenseTerm.AllowedPlacements);
                var restrictedPlacements = await GetLicensePlacementsKeysAsync(rawLicenseTerm.RestrictedPlacements);

                var allowedDistributions = await GetLicenseDistributionsKeysAsync(rawLicenseTerm.AllowedDistributions);
                var restrictedDistributions = await GetLicenseDistributionsKeysAsync(rawLicenseTerm.RestrictedDistributions);

                var allowedGeographies = await GetLicenseGeographiesKeysAsync(rawLicenseTerm.AllowedGeographies);
                var restrictedGeographies = await GetLicenseGeographiesKeysAsync(rawLicenseTerm.RestrictedGeographies);

                var allowedIndustries = await GetLicenseIndustriesKeysAsync(rawLicenseTerm.AllowedIndustries);
                var restrictedIndustries = await GetLicenseIndustriesKeysAsync(rawLicenseTerm.RestrictedIndustries);

                var allowedLanguages = await GetLicenseLanguagesKeysAsync(rawLicenseTerm.AllowedLanguages);
                var restrictedLanguages = await GetLicenseLanguagesKeysAsync(rawLicenseTerm.RestrictedLanguages);

                var usageLimits = await GetLicenseUsageLimitsKeysAsync(rawLicenseTerm.UsageLimits);

                var targetLicenseTerm = _syncTarget.CreateSyncLicenseTerm();

                if (rawLicenseTerm.SequenceNumber != null)
                    targetLicenseTerm.SetSequenceNumber((int)rawLicenseTerm.SequenceNumber);

                if (rawLicenseTerm.Name?.Count > 0)
                    targetLicenseTerm.SetName(rawLicenseTerm.Name);

                if (exclusivities?.Count() > 0)
                    targetLicenseTerm.SetExclusivities(exclusivities);

                if (allowedUsages?.Count() > 0)
                    targetLicenseTerm.SetAllowedUsages(allowedUsages);

                if (restrictedUsages?.Count() > 0)
                    targetLicenseTerm.SetRestrictedUsages(restrictedUsages);

                if (allowedSizes?.Count() > 0)
                    targetLicenseTerm.SetAllowedSizes(allowedSizes);

                if (restrictedSizes?.Count() > 0)
                    targetLicenseTerm.SetRestrictedSizes(restrictedSizes);

                if (allowedPlacements?.Count() > 0)
                    targetLicenseTerm.SetAllowedPlacements(allowedPlacements);

                if (restrictedPlacements?.Count() > 0)
                    targetLicenseTerm.SetRestrictedPlacements(restrictedPlacements);

                if (allowedDistributions?.Count() > 0)
                    targetLicenseTerm.SetAllowedDistributions(allowedDistributions);

                if (restrictedDistributions?.Count() > 0)
                    targetLicenseTerm.SetRestrictedDistributions(restrictedDistributions);

                if (allowedGeographies?.Count() > 0)
                    targetLicenseTerm.SetAllowedGeographies(allowedGeographies);

                if (restrictedGeographies?.Count() > 0)
                    targetLicenseTerm.SetRestrictedGeographies(restrictedGeographies);

                if (allowedIndustries?.Count() > 0)
                    targetLicenseTerm.SetAllowedIndustries(allowedIndustries);

                if (restrictedIndustries?.Count() > 0)
                    targetLicenseTerm.SetRestrictedIndustries(restrictedIndustries);

                if (allowedLanguages?.Count() > 0)
                    targetLicenseTerm.SetAllowedLanguages(allowedLanguages);

                if (restrictedLanguages?.Count() > 0)
                    targetLicenseTerm.SetRestrictedLanguages(restrictedLanguages);

                if (usageLimits?.Count() > 0)
                    targetLicenseTerm.SetUsageLimits(usageLimits);

                if (rawLicenseTerm.ValidFrom != null)
                    targetLicenseTerm.SetValidFrom((DateTimeOffset)rawLicenseTerm.ValidFrom);

                if (rawLicenseTerm.ValidUntil != null)
                    targetLicenseTerm.SetValidUntil((DateTimeOffset)rawLicenseTerm.ValidUntil);

                if (rawLicenseTerm.ToBeUsedUntil != null)
                    targetLicenseTerm.SetToBeUsedUntil((DateTimeOffset)rawLicenseTerm.ToBeUsedUntil);

                if (rawLicenseTerm.IsEditorialUse != null)
                    targetLicenseTerm.SetIsEditorialUse((bool)rawLicenseTerm.IsEditorialUse);

                targetLicenseTerms.Add(targetLicenseTerm);
            }

            return targetLicenseTerms;
        }

        private async Task<IList<string>> GetLicenseExclusivitiesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseExclusivityKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseUsagesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseUsageKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseSizesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseSizeKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicensePlacementsKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicensePlacementKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseDistributionsKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseDistributionKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseGeographiesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseGeographyKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseIndustriesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseIndustryKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseLanguagesKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseLanguageKeyAsync(smintIoKey));
            }

            return targetKeys;
        }

        private async Task<IList<string>> GetLicenseUsageLimitsKeysAsync(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(await _syncTarget.GetLicenseUsageLimitKeyAsync(smintIoKey));
            }

            return targetKeys;
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
