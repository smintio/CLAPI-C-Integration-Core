using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public abstract class SyncAssetImpl<TSyncAsset, TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints> : ISyncAsset<TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncAsset : ISyncAsset<TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseOption : SyncLicenseOptionImpl
        where TSyncLicenseTerm : SyncLicenseTermImpl
        where TSyncReleaseDetails : SyncReleaseDetailsImpl
        where TSyncDownloadConstraints : SyncDownloadConstraintsImpl
    {
        public string Uuid { get; set; }

        public string TargetAssetUuid { get; set; }

        public IDictionary<string, string> Name { get; set; }

        public string BinaryUuid { get; set; }
        public int BinaryVersion { get; set; }

        public IDictionary<string, string> BinaryUsage { get; set; }

        public string RecommendedFileName { get; set; }

        public string DownloadUrl { get; set; }

        public string LocalFileName { get; set; }

        public bool IsCompoundAsset { get; set; }

        public IList<TSyncAsset> AssetParts { get; set; }

        public string WorldwideUniqueBinaryUuid { get => $"{Uuid}_{BinaryUuid}"; }

        public SyncAssetImpl()
        {
            IsCompoundAsset = false;
        }

        public void SetUuid(string uuid)
        {
            Uuid = uuid;
        }

        public void SetTargetAssetUuid(string targetAssetUuid)
        {
            TargetAssetUuid = targetAssetUuid;
        }

        internal void SetNameInternal(IDictionary<string, string> name)
        {
            Name = name;

            SetName(name);
        }

        internal void SetBinaryUuidInternal(string binaryUuid)
        {
            BinaryUuid = binaryUuid;

            SetBinaryUuid(binaryUuid);
        }

        internal void SetBinaryVersionInternal(int binaryVersion)
        {
            BinaryVersion = binaryVersion;

            SetBinaryVersion(binaryVersion);
        }

        internal void SetBinaryUsageInternal(IDictionary<string, string> binaryUsage)
        {
            BinaryUsage = binaryUsage;

            SetBinaryUsage(binaryUsage);
        }

        public void SetRecommendedFileName(string recommendedFileName)
        {
            RecommendedFileName = recommendedFileName;
        }

        public void SetDownloadUrl(string downloadUrl)
        {
            DownloadUrl = downloadUrl;
        }

        public void SetAssetParts(IList<TSyncAsset> assetParts)
        {
            IsCompoundAsset = assetParts != null ? AssetParts.Count > 0 : false;

            AssetParts = assetParts;
        }

        public abstract void SetContentElementUuid(string contentElementUuid);

        public abstract void SetContentType(string contentTypeKey);
        public abstract void SetContentProvider(string contentProviderKey);
        public abstract void SetContentCategory(string contentCategoryKey);

        public abstract void SetName(IDictionary<string, string> name);
        public abstract void SetDescription(IDictionary<string, string> description);

        public abstract void SetSmintIoUrl(string smintIoUrl);

        public abstract void SetCreatedAt(DateTimeOffset createdAt);
        public abstract void SetLastUpdatedAt(DateTimeOffset lastUpdatedAt);
        public abstract void SetPurchasedAt(DateTimeOffset purchasedAt);

        public abstract void SetCartPurchaseTransactionUuid(string cartPurchaseTransactionUuid);
        public abstract void SetLicensePurchaseTransactionUuid(string licensePurchaseTransactionUuid);

        public abstract void SetHasBeenCancelled(bool hasBeenCancelled);

        public abstract void SetBinaryUuid(string binaryUuid);
        public abstract void SetBinaryType(string binaryTypeKey);
        public abstract void SetBinaryUsage(IDictionary<string, string> binaryUsage);
        public abstract void SetBinaryCulture(string binaryCulture);
        public abstract void SetBinaryVersion(int binaryVersion);

        public abstract void SetProjectUuid(string projectUuid);
        public abstract void SetProjectName(IDictionary<string, string> projectName);

        public abstract void SetCollectionUuid(string collectionUuid);
        public abstract void SetCollectionName(IDictionary<string, string> collectionName);

        public abstract void SetKeywords(IDictionary<string, string[]> keywords);

        public abstract void SetCopyrightNotices(IDictionary<string, string> copyrightNotices);

        public abstract void SetIsEditorialUse(bool isEditorialUse);
        public abstract void SetHasLicenseTerms(bool hasLicenseTerms);

        public abstract void SetLicenseType(string licenseTypeKey);

        public abstract void SetLicenseeUuid(string licenseeUuid);
        public abstract void SetLicenseeName(string licenseeName);

        public abstract void SetLicenseText(IDictionary<string, string> licenseText);

        public abstract void SetLicenseOptions(IList<TSyncLicenseOption> licenseOptions);
        public abstract void SetLicenseTerms(IList<TSyncLicenseTerm> licenseTerms);

        public abstract void SetDownloadConstraints(TSyncDownloadConstraints downloadConstraints);
        public abstract void SetReleaseDetails(TSyncReleaseDetails releaseDetails);
    }
}
