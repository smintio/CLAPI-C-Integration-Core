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
using System.Net;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;
using SmintIo.CLAPI.Consumer.Integration.Core.Target;
using SmintIo.CLAPI.Consumer.Integration.Core.Database.Models;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl
{
    internal class SyncJobImpl<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> : ISyncJob
        where TSyncAsset : BaseSyncAsset<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseTerm : ISyncLicenseTerm
        where TSyncReleaseDetails : ISyncReleaseDetails
        where TSyncDownloadConstraints : ISyncDownloadConstraints
    {
        private const string Folder = "temp";

        private readonly ISmintIoSettingsDatabaseProvider _smintIoSettingsDatabaseProvider;
        private readonly ISmintIoTokenDatabaseProvider _smintIoTokenDatabaseProvider;
        private readonly ISyncDatabaseProvider _syncDatabaseProvider;

        private readonly ISmintIoApiClientProvider _smintIoClient;

        private readonly ISyncTargetAuthenticator _syncTargetAuthenticator;

        private readonly ISyncTargetDataFactory<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> _syncTargetDataFactory;
        private readonly ISyncTarget<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> _syncTarget;

        private readonly ILogger _logger;

        private Dictionary<string, string> _contentProviderCache;

        private Dictionary<string, string> _contentTypeCache;
        private Dictionary<string, string> _binaryTypeCache;

        private Dictionary<string, string> _contentCategoryCache;

        private Dictionary<string, string> _licenseTypeCache;
        private Dictionary<string, string> _releaseStateCache;

        private Dictionary<string, string> _licenseExclusivityCache;
        private Dictionary<string, string> _licenseUsageCache;
        private Dictionary<string, string> _licenseSizeCache;
        private Dictionary<string, string> _licensePlacementCache;
        private Dictionary<string, string> _licenseDistributionCache;
        private Dictionary<string, string> _licenseGeographyCache;
        private Dictionary<string, string> _licenseIndustryCache;
        private Dictionary<string, string> _licenseLanguageCache;
        private Dictionary<string, string> _licenseUsageLimitCache;

        public SyncJobImpl(
            ISmintIoSettingsDatabaseProvider smintIoSettingsDatabaseProvider,
            ISmintIoTokenDatabaseProvider smintIoTokenDatabaseProvider,
            ISyncDatabaseProvider syncDatabaseProvider,            
            ISmintIoApiClientProvider smintIoClient,
            ISyncTargetAuthenticator syncTargetAuthenticator,
            ISyncTargetDataFactory<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> syncTargetDataFactory,
            ISyncTarget<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> syncTarget,
            ILogger<SyncJobImpl<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>> logger)
        {
            _smintIoSettingsDatabaseProvider = smintIoSettingsDatabaseProvider;
            _smintIoTokenDatabaseProvider = smintIoTokenDatabaseProvider;
            _syncDatabaseProvider = syncDatabaseProvider;

            _smintIoClient = smintIoClient;

            _syncTargetAuthenticator = syncTargetAuthenticator;

            _syncTargetDataFactory = syncTargetDataFactory;
            _syncTarget = syncTarget;

            _logger = logger;
        }

        public async Task SynchronizeAsync(bool synchronizeGenericMetadata)
        {
            try
            {
                var smintIoSettingsDatabaseModel = await _smintIoSettingsDatabaseProvider.GetSmintIoSettingsDatabaseModelAsync();
                smintIoSettingsDatabaseModel.ValidateForSync();

                if (!_syncTarget.GetCapabilities().IsMultiLanguageSupported() &&
                    smintIoSettingsDatabaseModel.ImportLanguages.Length > 1)
                {
                    throw new SyncJobException(
                        SyncJobException.SyncJobError.Generic,
                        "SyncTarget supports only one language but multiple language are set to be synced!"
                    );
                }

                var smintIoTokenDatabaseModel = await _smintIoTokenDatabaseProvider.GetTokenDatabaseModelAsync();
                smintIoTokenDatabaseModel.ValidateForSync();

                await _syncTargetAuthenticator.InitializeAuthenticationAsync();

                var cancelTask = !await _syncTarget.BeforeSyncAsync();
                
                if (cancelTask)
                {
                    _logger.LogInformation("BeforeSync task terminated with 'false', indicating to abort sync");

                    return;
                }

                if (!_syncTarget.GetCapabilities().IsCustomizedMetadataSynchronization())
                {
                    if (synchronizeGenericMetadata ||
                        _contentProviderCache == null)
                    {
                        await SynchronizeGenericMetadataAsync();
                    }
                }
                
                await SynchronizeAssetsAsync();

                await _syncTarget.AfterSyncAsync();
            }
            catch (AuthenticatorException e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleAuthenticatorExceptionAsync(e);
            }
            catch (SyncJobException e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleSyncJobExceptionAsync(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in sync job");

                await _syncTarget.HandleSyncJobExceptionAsync(new SyncJobException(SyncJobException.SyncJobError.Generic, e.Message));
            }
        }

        private async Task SynchronizeGenericMetadataAsync()
        {
            _logger.LogInformation("Starting Smint.io generic metadata synchronization...");

            var cancelMetadataSync = !await _syncTarget.BeforeGenericMetadataSyncAsync();
            if (cancelMetadataSync &&
                _contentProviderCache != null)
            {
                _logger.LogInformation("BeforeGenericMetadataSyncAsync task aborted meta data sync");

                return;
            }

            ClearGenericMetadataCaches();

            var genericMetadata = await _smintIoClient.GetGenericMetadataAsync();

            await _syncTarget.ImportContentProvidersAsync(genericMetadata.ContentProviders);
            _contentProviderCache = MapTargetMetadataUuids(genericMetadata.ContentProviders);

            await _syncTarget.ImportContentTypesAsync(genericMetadata.ContentTypes);
            _contentTypeCache = MapTargetMetadataUuids(genericMetadata.ContentTypes);

            await _syncTarget.ImportBinaryTypesAsync(genericMetadata.BinaryTypes);
            _binaryTypeCache = MapTargetMetadataUuids(genericMetadata.BinaryTypes);

            await _syncTarget.ImportContentCategoriesAsync(genericMetadata.ContentCategories);
            _contentCategoryCache = MapTargetMetadataUuids(genericMetadata.ContentCategories);

            await _syncTarget.ImportLicenseTypesAsync(genericMetadata.LicenseTypes);
            _licenseTypeCache = MapTargetMetadataUuids(genericMetadata.LicenseTypes);

            await _syncTarget.ImportReleaseStatesAsync(genericMetadata.ReleaseStates);
            _releaseStateCache = MapTargetMetadataUuids(genericMetadata.ReleaseStates);

            await _syncTarget.ImportLicenseExclusivitiesAsync(genericMetadata.LicenseExclusivities);
            _licenseExclusivityCache = MapTargetMetadataUuids(genericMetadata.LicenseExclusivities);

            await _syncTarget.ImportLicenseUsagesAsync(genericMetadata.LicenseUsages);
            _licenseUsageCache = MapTargetMetadataUuids(genericMetadata.LicenseUsages);

            await _syncTarget.ImportLicenseSizesAsync(genericMetadata.LicenseSizes);
            _licenseSizeCache = MapTargetMetadataUuids(genericMetadata.LicenseSizes);

            await _syncTarget.ImportLicensePlacementsAsync(genericMetadata.LicensePlacements);
            _licensePlacementCache = MapTargetMetadataUuids(genericMetadata.LicensePlacements);

            await _syncTarget.ImportLicenseDistributionsAsync(genericMetadata.LicenseDistributions);
            _licenseDistributionCache = MapTargetMetadataUuids(genericMetadata.LicenseDistributions);

            await _syncTarget.ImportLicenseGeographiesAsync(genericMetadata.LicenseGeographies);
            _licenseGeographyCache = MapTargetMetadataUuids(genericMetadata.LicenseGeographies);

            await _syncTarget.ImportLicenseIndustriesAsync(genericMetadata.LicenseIndustries);
            _licenseIndustryCache = MapTargetMetadataUuids(genericMetadata.LicenseIndustries);

            await _syncTarget.ImportLicenseLanguagesAsync(genericMetadata.LicenseLanguages);
            _licenseLanguageCache = MapTargetMetadataUuids(genericMetadata.LicenseLanguages);

            await _syncTarget.ImportLicenseUsageLimitsAsync(genericMetadata.LicenseUsageLimits);
            _licenseUsageLimitCache = MapTargetMetadataUuids(genericMetadata.LicenseUsageLimits);

            await _syncTarget.AfterGenericMetadataSyncAsync();

            _logger.LogInformation("Finished Smint.io generic metadata synchronization");
        }

        private Dictionary<string, string> MapTargetMetadataUuids(IList<SmintIoMetadataElement> metadataElements)
        {
            if (metadataElements == null)
                return new Dictionary<string, string>();

            return metadataElements.ToDictionary(metadataElement => metadataElement.Key,
                metadataElement => {
                    if (string.IsNullOrEmpty(metadataElement.TargetMetadataUuid))
                        throw new SyncJobException(
                            SyncJobException.SyncJobError.Generic,
                            $"SyncTarget did not return target metadata UUID for metadata element key {metadataElement.Key}!"
                     );

                    return metadataElement.TargetMetadataUuid;
                });
        }

        private async Task SynchronizeAssetsAsync()
        {
            _logger.LogInformation("Starting Smint.io asset synchronization...");

            var cancelAssetsSync = !await _syncTarget.BeforeAssetsSyncAsync();
            if (cancelAssetsSync)
            {
                _logger.LogInformation("BeforeAssetsSyncAsync task aborted assets sync");

                return;
            }

            var folderName = Folder + new Random().Next(1000000, 9999999);

            var syncDatabaseModel = await _syncDatabaseProvider.GetSyncDatabaseModelAsync();

            // get last committed state

            string continuationUuid = syncDatabaseModel?.ContinuationUuid;

            try
            {
                IList<SmintIoAsset> rawAssets = null;
                bool hasAssets;

                bool compoundAssetsSupported = _syncTarget.GetCapabilities().IsCompoundAssetsSupported();
                bool binaryUpdatesSupported = _syncTarget.GetCapabilities().IsBinaryUpdatesSupported();

                do
                {
                    (rawAssets, continuationUuid, hasAssets) = await _smintIoClient.GetAssetsAsync(continuationUuid, compoundAssetsSupported, binaryUpdatesSupported);

                    if (rawAssets != null && rawAssets.Any())
                    {
                        CreateTempFolder(folderName);

                        var targetAssets = TransformAssets(rawAssets, folderName);

                        IList<TSyncAsset> newTargetAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> updatedTargetAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> newTargetCompoundAssets = new List<TSyncAsset>();
                        IList<TSyncAsset> updatedTargetCompoundAssets = new List<TSyncAsset>();

                        foreach (var targetAsset in targetAssets)
                        {
                            if (targetAsset.IsCompoundAsset)
                            {
                                string targetCompoundAssetUuid = await _syncTarget.GetTargetCompoundAssetUuidAsync(targetAsset.Uuid, targetAsset.RecommendedFileName);

                                if (!string.IsNullOrEmpty(targetCompoundAssetUuid))
                                {
                                    targetAsset.SetTargetAssetUuid(targetCompoundAssetUuid);

                                    updatedTargetCompoundAssets.Add(targetAsset);
                                }
                                else
                                {
                                    newTargetCompoundAssets.Add(targetAsset);
                                }
                            }
                            else
                            {
                                string targetAssetUuid;

                                if (_syncTarget.GetCapabilities().IsHandleReuse() &&
                                    !string.IsNullOrEmpty(targetAsset.ReusedUuid))
                                {
                                    // target is able to handle reuses of LPTs

                                    targetAssetUuid = await _syncTarget.GetTargetAssetBinaryUuidAsync(targetAsset.ReusedUuid, targetAsset.BinaryUuid, targetAsset.RecommendedFileName);

                                    // make sure we know when there is some issue

                                    if (string.IsNullOrEmpty(targetAssetUuid))
                                        throw new Exception($"Target asset for reuse not found (${targetAsset.ReusedUuid})");
                                }
                                else
                                {
                                    targetAssetUuid = await _syncTarget.GetTargetAssetBinaryUuidAsync(targetAsset.Uuid, targetAsset.BinaryUuid, targetAsset.RecommendedFileName);
                                }

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
                            await _syncTarget.ImportNewTargetAssetsAsync(newTargetAssets);

                        if (updatedTargetAssets.Count > 0)
                            await _syncTarget.UpdateTargetAssetsAsync(updatedTargetAssets);

                        if (newTargetCompoundAssets.Count > 0)
                            await _syncTarget.ImportNewTargetCompoundAssetsAsync(newTargetCompoundAssets);

                        if (updatedTargetCompoundAssets.Count > 0)
                            await _syncTarget.UpdateTargetCompoundAssetsAsync(updatedTargetCompoundAssets);
                        
                        // store committed data

                        await _syncDatabaseProvider.SetSyncDatabaseModelAsync(new SyncDatabaseModel()
                        {
                            ContinuationUuid = continuationUuid
                        });
                    }

                    _logger.LogInformation($"Synchronized {rawAssets.Count()} Smint.io assets");
                } while (hasAssets);

                _logger.LogInformation("Finished Smint.io asset synchronization");

                await _syncTarget.AfterAssetsSyncAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                RemoveTempFolder(folderName);
            }
        }

        private IList<TSyncAsset> TransformAssets(IList<SmintIoAsset> rawAssets, string temporaryFolder)
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

                    var targetAsset = _syncTargetDataFactory.CreateSyncBinaryAsset();

                    _syncTarget.PrepareTargetAsset(targetAsset);

                    targetAsset.SetAsset(rawAsset);

                    targetAsset.SetBinary(binary);

                    targetAsset.SetUuid(rawAsset.LicensePurchaseTransactionUuid);

                    targetAsset.SetRecommendedFileName(recommendedFileName);

                    if (_syncTarget.GetCapabilities().IsHandleReuse() &&
                        !string.IsNullOrEmpty(rawAsset.ReusedLicensePurchaseTransactionUuid))
                    {
                        // target supports reuse of LPTs

                        targetAsset.SetReusedUuid(rawAsset.ReusedLicensePurchaseTransactionUuid);
                    }

                    string localFileName = $"{temporaryFolder}/{recommendedFileName}";
                    var targetFile = new FileInfo(localFileName);
                    targetAsset.SetDownloadedFileProvider(async () =>
                    {
                        _logger.LogInformation($"Downloading file UUID {targetAsset.RecommendedFileName} to {localFileName}...");

                        if (downloadUrl == null)
                        {
                            return null;
                        }

                        try
                        {
                            WebClient wc = new WebClient();
                            await wc.DownloadFileTaskAsync(downloadUrl, targetFile.FullName);
                        }
                        catch (WebException we)
                        {
                            _logger.LogError(we, "Error downloading asset");
                            throw;
                        }

                        _logger.LogInformation($"Downloaded file UUID {targetAsset.RecommendedFileName} to {localFileName}");

                        return targetFile;
                    });

                    SetContentMetadata(targetAsset, rawAsset, binary);
                    SetLicenseMetadata(targetAsset, rawAsset);

                    assetPartAssets.Add(targetAsset);

                    assets.Add(targetAsset);
                }

                if (assetPartAssets.Count > 1)
                {
                    // we have a compound asset, consisting of more than one asset part

                    var targetCompoundAsset = _syncTargetDataFactory.CreateSyncCompoundAsset();

                    targetCompoundAsset.SetUuid(rawAsset.LicensePurchaseTransactionUuid);

                    if (_syncTarget.GetCapabilities().IsHandleReuse() &&
                        !string.IsNullOrEmpty(rawAsset.ReusedLicensePurchaseTransactionUuid))
                    {
                        // target supportes reuse of LPTs

                        targetCompoundAsset.SetReusedUuid(rawAsset.ReusedLicensePurchaseTransactionUuid);
                    }

                    targetCompoundAsset.SetAssetParts(assetPartAssets);

                    SetContentMetadata(targetCompoundAsset, rawAsset, null);
                    SetLicenseMetadata(targetCompoundAsset, rawAsset);

                    assets.Add(targetCompoundAsset);
                }

                _logger.LogInformation($"Transformed Smint.io LPT {rawAsset.LicensePurchaseTransactionUuid}");
            }

            return assets;
        }

        private void SetContentMetadata(TSyncAsset targetAsset, SmintIoAsset rawAsset, SmintIoBinary binary)
        {
            var contentTypeString = !string.IsNullOrEmpty(binary?.ContentType) ? binary.ContentType : rawAsset.ContentType;

            targetAsset.SetContentElementUuid(rawAsset.ContentElementUuid);
            targetAsset.SetContentProvider(GetContentProviderKey(rawAsset.Provider));
            targetAsset.SetContentType(GetContentTypeKey(contentTypeString));
            targetAsset.SetContentCategory(GetContentCategoryKey(rawAsset.Category));
            targetAsset.SetSmintIoUrl(rawAsset.SmintIoUrl);
            targetAsset.SetPurchasedAt(rawAsset.PurchasedAt);
            targetAsset.SetCreatedAt(rawAsset.CreatedAt);
            targetAsset.SetCartUuid(rawAsset.CartPurchaseTransactionUuid);
            targetAsset.SetHasBeenCancelled(rawAsset.State == Client.Generated.LicensePurchaseTransactionStateEnum.Cancelled);

            if (!string.IsNullOrEmpty(binary?.BinaryType))
                targetAsset.SetBinaryType(GetBinaryTypeKey(binary.BinaryType));

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

        private void SetLicenseMetadata(TSyncAsset targetAsset, SmintIoAsset rawAsset)
        {
            targetAsset.SetLicenseType(GetLicenseTypeKey(rawAsset.LicenseType));

            targetAsset.SetLicenseeUuid(rawAsset.LicenseeUuid);
            targetAsset.SetLicenseeName(rawAsset.LicenseeName);

            if (rawAsset.LicenseText?.Count > 0)
                targetAsset.SetLicenseText(rawAsset.LicenseText);

            if (rawAsset.LicenseUrls?.Count > 0)
                targetAsset.SetLicenseUrls(rawAsset.LicenseUrls);

            if (rawAsset.LicenseTerms?.Count > 0)
                targetAsset.SetLicenseTerms(GetLicenseTerms(rawAsset.LicenseTerms));

            if (rawAsset.DownloadConstraints != null)
            {
                var rawDownloadConstraints = rawAsset.DownloadConstraints;

                var targetDownloadConstraints = _syncTargetDataFactory.CreateSyncDownloadConstraints();

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

                var targetReleaseDetails = _syncTargetDataFactory.CreateSyncReleaseDetails();

                string modelReleaseState = null;
                if (!string.IsNullOrEmpty(rawReleaseDetails.ModelReleaseState))
                    modelReleaseState = GetReleaseStateKey(rawReleaseDetails.ModelReleaseState);

                string propertyReleaseState = null;
                if (!string.IsNullOrEmpty(rawReleaseDetails.PropertyReleaseState))
                    propertyReleaseState = GetReleaseStateKey(rawReleaseDetails.PropertyReleaseState);

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

            if (rawAsset.HasRestrictiveLicenseTerms != null)
                targetAsset.SetHasRestrictiveLicenseTerms((bool)rawAsset.HasRestrictiveLicenseTerms);
        }

        private IList<TSyncLicenseTerm> GetLicenseTerms(IList<SmintIoLicenseTerm> rawLicenseTerms)
        {
            var targetLicenseTerms = new List<TSyncLicenseTerm>();

            foreach (var rawLicenseTerm in rawLicenseTerms)
            {
                var exclusivities = GetLicenseExclusivitiesKeys(rawLicenseTerm.Exclusivities);

                var allowedUsages = GetLicenseUsagesKeys(rawLicenseTerm.AllowedUsages);
                var restrictedUsages = GetLicenseUsagesKeys(rawLicenseTerm.RestrictedUsages);

                var allowedSizes = GetLicenseSizesKeys(rawLicenseTerm.AllowedSizes);
                var restrictedSizes = GetLicenseSizesKeys(rawLicenseTerm.RestrictedSizes);

                var allowedPlacements = GetLicensePlacementsKeys(rawLicenseTerm.AllowedPlacements);
                var restrictedPlacements = GetLicensePlacementsKeys(rawLicenseTerm.RestrictedPlacements);

                var allowedDistributions = GetLicenseDistributionsKeys(rawLicenseTerm.AllowedDistributions);
                var restrictedDistributions = GetLicenseDistributionsKeys(rawLicenseTerm.RestrictedDistributions);

                var allowedGeographies = GetLicenseGeographiesKeys(rawLicenseTerm.AllowedGeographies);
                var restrictedGeographies = GetLicenseGeographiesKeys(rawLicenseTerm.RestrictedGeographies);

                var allowedIndustries = GetLicenseIndustriesKeys(rawLicenseTerm.AllowedIndustries);
                var restrictedIndustries = GetLicenseIndustriesKeys(rawLicenseTerm.RestrictedIndustries);

                var allowedLanguages = GetLicenseLanguagesKeys(rawLicenseTerm.AllowedLanguages);
                var restrictedLanguages = GetLicenseLanguagesKeys(rawLicenseTerm.RestrictedLanguages);

                var usageLimits = GetLicenseUsageLimitsKeys(rawLicenseTerm.UsageLimits);

                var targetLicenseTerm = _syncTargetDataFactory.CreateSyncLicenseTerm();

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

        private string GetContentProviderKey(string smintIoKey)
        {
            if (_contentProviderCache == null)
                return smintIoKey;

            if (_contentProviderCache.ContainsKey(smintIoKey))
                return _contentProviderCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io content provider key {smintIoKey}!"
                 );
        }

        private string GetContentTypeKey(string smintIoKey)
        {
            if (_contentProviderCache == null)
                return smintIoKey;

            if (_contentTypeCache.ContainsKey(smintIoKey))
                return _contentTypeCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io content type key {smintIoKey}!"
                 );
        }

        private string GetContentCategoryKey(string smintIoKey)
        {
            if (_contentCategoryCache == null)
                return smintIoKey;

            if (_contentCategoryCache.ContainsKey(smintIoKey))
                return _contentCategoryCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io content category key {smintIoKey}!"
                 );
        }

        private string GetBinaryTypeKey(string smintIoKey)
        {
            if (_binaryTypeCache == null)
                return smintIoKey;

            if (_binaryTypeCache.ContainsKey(smintIoKey))
                return _binaryTypeCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io binary type key {smintIoKey}!"
                 );
        }

        private string GetLicenseTypeKey(string smintIoKey)
        {
            if (_licenseTypeCache == null)
                return smintIoKey;

            if (_licenseTypeCache.ContainsKey(smintIoKey))
                return _licenseTypeCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license type key {smintIoKey}!"
                 );
        }

        private string GetReleaseStateKey(string smintIoKey)
        {
            if (_releaseStateCache == null)
                return smintIoKey;

            if (_releaseStateCache.ContainsKey(smintIoKey))
                return _releaseStateCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io release state key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseExclusivitiesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseExclusivityKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseExclusivityKey(string smintIoKey)
        {
            if (_licenseExclusivityCache == null)
                return smintIoKey;

            if (_licenseExclusivityCache.ContainsKey(smintIoKey))
                return _licenseExclusivityCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license exclusivity key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseUsagesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseUsageKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseUsageKey(string smintIoKey)
        {
            if (_licenseUsageCache == null)
                return smintIoKey;

            if (_licenseUsageCache.ContainsKey(smintIoKey))
                return _licenseUsageCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license usage key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseSizesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseSizeKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseSizeKey(string smintIoKey)
        {
            if (_licenseSizeCache == null)
                return smintIoKey;

            if (_licenseSizeCache.ContainsKey(smintIoKey))
                return _licenseSizeCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license size key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicensePlacementsKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicensePlacementKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicensePlacementKey(string smintIoKey)
        {
            if (_licensePlacementCache == null)
                return smintIoKey;

            if (_licensePlacementCache.ContainsKey(smintIoKey))
                return _licensePlacementCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license placement key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseDistributionsKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseDistributionKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseDistributionKey(string smintIoKey)
        {
            if (_licenseDistributionCache == null)
                return smintIoKey;

            if (_licenseDistributionCache.ContainsKey(smintIoKey))
                return _licenseDistributionCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license distribution key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseGeographiesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseGeographyKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseGeographyKey(string smintIoKey)
        {
            if (_licenseGeographyCache == null)
                return smintIoKey;

            if (_licenseGeographyCache.ContainsKey(smintIoKey))
                return _licenseGeographyCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license geography key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseIndustriesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseIndustryKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseIndustryKey(string smintIoKey)
        {
            if (_licenseIndustryCache == null)
                return smintIoKey;

            if (_licenseIndustryCache.ContainsKey(smintIoKey))
                return _licenseIndustryCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license industry key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseLanguagesKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseLanguageKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseLanguageKey(string smintIoKey)
        {
            if (_licenseLanguageCache == null)
                return smintIoKey;

            if (_licenseLanguageCache.ContainsKey(smintIoKey))
                return _licenseLanguageCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license language key {smintIoKey}!"
                 );
        }

        private IList<string> GetLicenseUsageLimitsKeys(IList<string> smintIoKeys)
        {
            if (smintIoKeys == null || !smintIoKeys.Any())
                return null;

            var targetKeys = new List<string>();

            foreach (var smintIoKey in smintIoKeys)
            {
                targetKeys.Add(GetLicenseUsageLimitKey(smintIoKey));
            }

            return targetKeys;
        }

        private string GetLicenseUsageLimitKey(string smintIoKey)
        {
            if (_licenseUsageLimitCache == null)
                return smintIoKey;

            if (_licenseUsageLimitCache.ContainsKey(smintIoKey))
                return _licenseUsageLimitCache[smintIoKey];

            throw new SyncJobException(
                     SyncJobException.SyncJobError.Generic,
                     $"SyncTarget did not return key mapping covering Smint.io license usage limit key {smintIoKey}!"
                 );
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

        private void ClearGenericMetadataCaches()
        {
            _contentProviderCache = null;

            _contentTypeCache = null;
            _binaryTypeCache = null;

            _contentCategoryCache = null;

            _licenseTypeCache = null;
            _releaseStateCache = null;

            _licenseExclusivityCache = null;
            _licenseUsageCache = null;
            _licenseSizeCache = null;
            _licensePlacementCache = null;
            _licenseDistributionCache = null;
            _licenseGeographyCache = null;
            _licenseIndustryCache = null;
            _licenseLanguageCache = null;
            _licenseUsageLimitCache = null;
        }
    }
}
