using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SmintIo.CLAPI.Consumer.Client.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using System.Net;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;
using SmintIo.CLAPI.Consumer.Integration.Core.Database;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Providers.Impl
{
    internal class SmintIoApiClientProviderImpl: IDisposable, ISmintIoApiClientProvider
    {
        private const int MaxRetryAttempts = 5;

        private readonly ISettingsDatabaseProvider _settingsDatabaseProvider;
        private readonly ITokenDatabaseProvider _tokenDatabaseProvider;

        private readonly ISmintIoAuthenticator _authenticator;

        private readonly HttpClient _http;

        private readonly AsyncRetryPolicy _retryPolicy;

        private bool _disposed;

        private readonly ILogger _logger;

        private readonly CLAPICOpenApiClient _clapicOpenApiClient;

        public SmintIoApiClientProviderImpl(
            ISettingsDatabaseProvider settingsDatabaseProvider,
            ITokenDatabaseProvider tokenDatabaseProvider,
            ILogger<SmintIoApiClientProviderImpl> logger,
            ISmintIoAuthenticator authenticator)
        {
            _settingsDatabaseProvider = settingsDatabaseProvider;
            _tokenDatabaseProvider = tokenDatabaseProvider;

            _authenticator = authenticator;
            
            _disposed = false;

            _http = new HttpClient();

            _logger = logger;

            _retryPolicy = GetRetryStrategy();

            _clapicOpenApiClient = new CLAPICOpenApiClient(_http);            
        }

        public async Task<SmintIoGenericMetadata> GetGenericMetadataAsync()
        {
            _logger.LogInformation("Receiving generic metadata from Smint.io...");

            await SetupClapicOpenApiClientAsync();

            var syncGenericMetadata = await _retryPolicy.ExecuteAsync(async () =>
                await _clapicOpenApiClient.GetGenericMetadataForSyncAsync());

            var smintIoGenericMetadata = new SmintIoGenericMetadata();

            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();

            var importLanguages = settingsDatabaseModel.ImportLanguages;

            smintIoGenericMetadata.ContentProviders = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.Providers);

            smintIoGenericMetadata.ContentTypes = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.Content_types);
            smintIoGenericMetadata.BinaryTypes = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.Binary_types);

            smintIoGenericMetadata.ContentCategories = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.Content_categories);

            smintIoGenericMetadata.LicenseTypes = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_types);
            smintIoGenericMetadata.ReleaseStates = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.Release_states);

            smintIoGenericMetadata.LicenseUsages = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_usages);
            smintIoGenericMetadata.LicenseSizes = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_sizes);
            smintIoGenericMetadata.LicensePlacements = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_placements);
            smintIoGenericMetadata.LicenseDistributions = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_distributions);
            smintIoGenericMetadata.LicenseGeographies = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_geographies);
            smintIoGenericMetadata.LicenseVerticals = GetGroupedMetadataElementsForImportLanguages(importLanguages, syncGenericMetadata.License_verticals);

            _logger.LogInformation("Received generic metadata from Smint.io");

            return smintIoGenericMetadata;
        }

        private IList<SmintIoMetadataElement> GetGroupedMetadataElementsForImportLanguages(string[] importLanguages, LocalizedMetadataElements localizedMetadataElements)
        {
            return localizedMetadataElements
                .Where(localizedMetadataElement => importLanguages.Contains(localizedMetadataElement.Culture))
                .GroupBy(localizedMetadataElement => localizedMetadataElement.Metadata_element.Key)
                .Select((group) =>
                {
                    return new SmintIoMetadataElement()
                    {
                        Key = group.Key,
                        Values = group.ToDictionary(metadataElement => metadataElement.Culture, metadataElement => metadataElement.Metadata_element.Name)
                    };
                })
                .ToList();
        }

        public async Task<(IList<SmintIoAsset>, string)> GetAssetsAsync(string continuationUuid)
        {
            _logger.LogInformation("Receiving assets from Smint.io...");

            IList<SmintIoAsset> result;

            (result, continuationUuid) = await LoadAssetsAsync(continuationUuid);
            
            _logger.LogInformation($"Received {result.Count()} assets from Smint.io");

            return (result, continuationUuid);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private AsyncRetryPolicy GetRetryStrategy()
        {
            return Policy
                .Handle<ApiException>()
                .Or<Exception>()
                .WaitAndRetryAsync(
                    MaxRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    async (ex, timespan, context) =>
                    {
                        _logger.LogError(ex, "Error communicating to Smint.io");

                        if (ex is ApiException apiEx)
                        {
                            if (apiEx.StatusCode == (int)HttpStatusCode.Forbidden || apiEx.StatusCode == (int)HttpStatusCode.Unauthorized)
                            {
                                await _authenticator.RefreshSmintIoTokenAsync();

                                // backoff and try again 

                                return;
                            }
                            else if (apiEx.StatusCode == (int)HttpStatusCode.TooManyRequests)
                            {
                                // backoff and try again

                                return;
                            }

                            // expected error happened server side, most likely our problem, cancel

                            throw ex;
                        }

                        // some server side or communication issue, backoff and try again
                    });
        }

        private async Task<(IList<SmintIoAsset>, string)> LoadAssetsAsync(string continuationUuid)
        {
            await SetupClapicOpenApiClientAsync();

            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();

            IList<SmintIoAsset> assets = new List<SmintIoAsset>();

            SyncLicensePurchaseTransactionQueryResult syncLptQueryResult = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _clapicOpenApiClient.GetLicensePurchaseTransactionsForSyncAsync(
                    continuationUuid: continuationUuid,
                    limit: 10);
            });

            if (syncLptQueryResult.Count == 0)
            {
                return (assets, syncLptQueryResult.Continuation_uuid);
            }

            foreach (var lpt in syncLptQueryResult.License_purchase_transactions)
            {
                bool? isEditorialUse = null;
                bool? hasLicenseUsageConstraints = false;

                foreach (var license_usage_constraint in lpt.License_usage_constraints)
                {
                    // make sure we do not store editorial use information if no information is there!

                    if (license_usage_constraint.Effective_is_editorial_use != null)
                    {
                        if (license_usage_constraint.Effective_is_editorial_use == true)
                        {
                            // if we have a restrictions, always indicate

                            isEditorialUse = true;
                        }
                        else if (license_usage_constraint.Effective_is_editorial_use == false)
                        {
                            // if we have no restriction, only store, if we have no other restriction

                            if (isEditorialUse == null)
                                isEditorialUse = false;
                        }
                    }
                    
                    hasLicenseUsageConstraints |=
                        license_usage_constraint.Effective_restricted_usages?.Count > 0 ||
                        license_usage_constraint.Effective_restricted_sizes?.Count > 0 ||
                        license_usage_constraint.Effective_restricted_placements?.Count > 0 ||
                        license_usage_constraint.Effective_restricted_distributions?.Count > 0 ||
                        license_usage_constraint.Effective_restricted_geographies?.Count > 0 ||
                        license_usage_constraint.Effective_restricted_verticals?.Count > 0 ||
                        (license_usage_constraint.Effective_valid_from != null && license_usage_constraint.Effective_valid_from > DateTimeOffset.Now) ||
                        license_usage_constraint.Effective_valid_until != null ||
                        license_usage_constraint.Effective_to_be_used_until != null ||
                        (license_usage_constraint.Effective_is_editorial_use ?? false);
                }

                string url = $"https://{settingsDatabaseModel.TenantId}.smint.io/project/{lpt.Project_uuid}/content-element/{lpt.Content_element.Uuid}";

                var importLanguages = settingsDatabaseModel.ImportLanguages;

                var asset = new SmintIoAsset()
                {
                    LicensePurchaseTransactionUuid = lpt.Uuid,
                    CartPurchaseTransactionUuid = lpt.Cart_purchase_transaction_uuid,                    
                    State = lpt.State,
                    Provider = lpt.Content_element.Provider,
                    ContentType = lpt.Content_element.Content_type,
                    Name = GetValuesForImportLanguages(importLanguages, lpt.Content_element.Name),
                    Description = GetValuesForImportLanguages(importLanguages, lpt.Content_element.Description),
                    Keywords = GetGroupedValuesForImportLanguages(importLanguages, lpt.Content_element.Keywords),
                    Category = lpt.Content_element.Content_category,
                    ReleaseDetails = GetReleaseDetails(importLanguages, lpt),
                    CopyrightNotices = GetValuesForImportLanguages(importLanguages, lpt.Content_element.Copyright_notices),
                    ProjectUuid = lpt.Project_uuid,
                    ProjectName = GetValuesForImportLanguages(importLanguages, lpt.Project_name),
                    CollectionUuid = lpt.Collection_uuid,
                    CollectionName = GetValuesForImportLanguages(importLanguages, lpt.Collection_name),
                    LicenseeUuid = lpt.Licensee_uuid,
                    LicenseeName = lpt.Licensee_name,
                    LicenseType = lpt.Offering.License_type,
                    LicenseText = GetValuesForImportLanguages(importLanguages, lpt.Offering.License_text.Effective_text),
                    LicenseOptions = GetLicenseOptions(importLanguages, lpt),
                    UsageConstraints = GetUsageConstraints(lpt),
                    DownloadConstraints = GetDownloadConstraints(lpt),
                    EffectiveIsEditorialUse = isEditorialUse,
                    EffectiveHasLicenseUsageConstraints = hasLicenseUsageConstraints,
                    SmintIoUrl = url,
                    PurchasedAt = lpt.Purchased_at,
                    CreatedAt = lpt.Created_at,
                    LastUpdatedAt = lpt.Last_updated_at ?? lpt.Created_at ?? DateTimeOffset.Now,
                };

                if (lpt.Can_be_synced ?? false)
                {
                    var syncBinaries =
                        await _clapicOpenApiClient.GetLicensePurchaseTransactionBinariesForSyncAsync(asset.CartPurchaseTransactionUuid, asset.LicensePurchaseTransactionUuid);

                    asset.Binaries = GetBinaries(importLanguages, syncBinaries);

                    assets.Add(asset);
                }
            }

            return (assets, syncLptQueryResult.Continuation_uuid);
        }

        private List<SmintIoBinary> GetBinaries(string[] importLanguages, IList<SyncBinary> syncBinaries)
        {
            List<SmintIoBinary> smintIoBinaries = new List<SmintIoBinary>();

            foreach (var syncBinary in syncBinaries)
            {
                smintIoBinaries.Add(new SmintIoBinary()
                {
                    Uuid = syncBinary.Uuid,
                    ContentType = syncBinary.Content_type,
                    BinaryType = syncBinary.Binary_type,
                    Name = GetValuesForImportLanguages(importLanguages, syncBinary.Name),
                    Description = GetValuesForImportLanguages(importLanguages, syncBinary.Description),
                    Usage = GetValuesForImportLanguages(importLanguages, syncBinary.Usage),
                    DownloadUrl = syncBinary.Download_url,
                    RecommendedFileName = syncBinary.Recommended_file_name,
                    Culture = syncBinary.Culture,
                    Version = syncBinary.Version ?? 1
                });
            }

            return smintIoBinaries;
        }

        private List<SmintIoLicenseOptions> GetLicenseOptions(string[] importLanguages, SyncLicensePurchaseTransaction lpt)
        {
            if (!(lpt.Offering.Has_options ?? false))
            {
                return null;
            }

            List<SmintIoLicenseOptions> options = new List<SmintIoLicenseOptions>();

            foreach (var option in lpt.Offering.Options)
            {
                options.Add(new SmintIoLicenseOptions()
                {
                    OptionName = GetValuesForImportLanguages(importLanguages, option.Option_name),
                    LicenseText = GetValuesForImportLanguages(importLanguages, option.License_text.Effective_text)
                });
            }

            return options;
        }

        private List<SmintIoUsageConstraints> GetUsageConstraints(SyncLicensePurchaseTransaction lpt)
        {
            if (lpt.License_usage_constraints == null || lpt.License_usage_constraints.Count == 0)
            {
                return null;
            }

            List<SmintIoUsageConstraints> licenseUsageConstraints = new List<SmintIoUsageConstraints>();

            foreach (var licenseUsageConstraint in lpt.License_usage_constraints)
            {
                licenseUsageConstraints.Add(new SmintIoUsageConstraints()
                {
                    EffectiveIsExclusive = licenseUsageConstraint.Effective_is_exclusive,
                    EffectiveAllowedUsages = licenseUsageConstraint.Effective_allowed_usages,
                    EffectiveRestrictedUsages = licenseUsageConstraint.Effective_restricted_usages,
                    EffectiveAllowedSizes = licenseUsageConstraint.Effective_allowed_sizes,
                    EffectiveRestrictedSizes = licenseUsageConstraint.Effective_restricted_sizes,
                    EffectiveAllowedPlacements = licenseUsageConstraint.Effective_allowed_placements,
                    EffectiveRestrictedPlacements = licenseUsageConstraint.Effective_restricted_placements,
                    EffectiveAllowedDistributions = licenseUsageConstraint.Effective_allowed_distributions,
                    EffectiveRestrictedDistributions = licenseUsageConstraint.Effective_restricted_distributions,
                    EffectiveAllowedGeographies = licenseUsageConstraint.Effective_allowed_geographies,
                    EffectiveRestrictedGeographies = licenseUsageConstraint.Effective_restricted_geographies,
                    EffectiveAllowedVerticals = licenseUsageConstraint.Effective_allowed_verticals,
                    EffectiveRestrictedVerticals = licenseUsageConstraint.Effective_restricted_verticals,
                    EffectiveToBeUsedUntil = licenseUsageConstraint.Effective_to_be_used_until,
                    EffectiveValidFrom = licenseUsageConstraint.Effective_valid_from,
                    EffectiveValidUntil = licenseUsageConstraint.Effective_valid_until,
                    EffectiveIsEditorialUse = licenseUsageConstraint.Effective_is_editorial_use
                });
            }

            return licenseUsageConstraints;
        }

        private SmintIoDownloadConstraints GetDownloadConstraints(SyncLicensePurchaseTransaction lpt)
        {
            if (lpt.License_download_constraints == null)
            {
                return null;
            }

            var licenseDownloadConstraints = lpt.License_download_constraints;

            return new SmintIoDownloadConstraints()
            {
                EffectiveMaxDownloads = licenseDownloadConstraints.Effective_max_downloads,
                EffectiveMaxUsers = licenseDownloadConstraints.Effective_max_users,
                EffectiveMaxReuses = licenseDownloadConstraints.Effective_max_reuses
            };
        }

        private SmintIoReleaseDetails GetReleaseDetails(string[] importLanguages, SyncLicensePurchaseTransaction lpt)
        {
            if (lpt.Content_element.Release_details == null)
            {
                return null;
            }

            var releaseDetails = lpt.Content_element.Release_details;

            return new SmintIoReleaseDetails()
            {
                ModelReleaseState = releaseDetails.Model_release_state,
                PropertyReleaseState = releaseDetails.Property_release_state,
                ProviderAllowedUseComment = GetValuesForImportLanguages(importLanguages, releaseDetails.Provider_allowed_use_comment),
                ProviderReleaseComment = GetValuesForImportLanguages(importLanguages, releaseDetails.Provider_release_comment),
                ProviderUsageConstraints = GetValuesForImportLanguages(importLanguages, releaseDetails.Provider_usage_constraints)
            };
        }

        private IDictionary<string, string[]> GetGroupedValuesForImportLanguages(string[] importLanguages, LocalizedMetadataElements localizedMetadataElements)
        {
            if (localizedMetadataElements == null)
                return null;

            return localizedMetadataElements
                .Where(localizedMetadataElement => importLanguages.Contains(localizedMetadataElement.Culture))
                .GroupBy(localizedMetadataElement => localizedMetadataElement.Culture)
                .ToDictionary(group => group.Key, group => group.Select(localizedMetadataElement => localizedMetadataElement.Metadata_element.Name).ToArray());
        }

        private IDictionary<string, string> GetValuesForImportLanguages(string[] importLanguages, LocalizedStrings localizedStrings)
        {
            if (localizedStrings == null)
                return null;

            return localizedStrings
                .Where(localizedString => importLanguages.Contains(localizedString.Culture))
                .ToDictionary(localizedString => localizedString.Culture, localizedString => localizedString.Value);
        }

        private async Task SetupClapicOpenApiClientAsync()
        {
            var settingsDatabaseModel = await _settingsDatabaseProvider.GetSettingsDatabaseModelAsync();
            var tokenDatabaseModel = await _tokenDatabaseProvider.GetTokenDatabaseModelAsync();

            _clapicOpenApiClient.BaseUrl = $"https://{settingsDatabaseModel.TenantId}-clapi.smint.io/consumer/v1";
            _clapicOpenApiClient.AccessToken = tokenDatabaseModel.AccessToken;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _http?.Dispose();
            }

            _disposed = true;
        }
    }
}
