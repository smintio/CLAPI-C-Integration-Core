using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;
using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncAsset<TSyncLicenseOption, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseOption : SyncLicenseOptionImpl
        where TSyncLicenseTerm : SyncLicenseTermImpl
        where TSyncReleaseDetails : SyncReleaseDetailsImpl
        where TSyncDownloadConstraints : SyncDownloadConstraintsImpl
    {
        void SetContentElementUuid(string contentElementUuid);

        void SetContentType(string contentTypeKey);
        void SetContentProvider(string contentProviderKey);
        void SetContentCategory(string contentCategoryKey);

        void SetName(IDictionary<string, string> name);
        void SetDescription(IDictionary<string, string> description);

        void SetSmintIoUrl(string smintIoUrl);

        void SetCreatedAt(DateTimeOffset createdAt);
        void SetLastUpdatedAt(DateTimeOffset lastUpdatedAt);
        void SetPurchasedAt(DateTimeOffset purchasedAt);

        void SetCartPurchaseTransactionUuid(string cartPurchaseTransactionUuid);
        void SetLicensePurchaseTransactionUuid(string licensePurchaseTransactionUuid);

        void SetHasBeenCancelled(bool hasBeenCancelled);

        void SetBinaryUuid(string binaryUuid);        
        void SetBinaryType(string binaryTypeKey);
        void SetBinaryUsage(IDictionary<string, string> binaryUsage);
        void SetBinaryCulture(string binaryCulture);
        void SetBinaryVersion(int binaryVersion);

        void SetProjectUuid(string projectUuid);
        void SetProjectName(IDictionary<string, string> projectName);

        void SetCollectionUuid(string collectionUuid);
        void SetCollectionName(IDictionary<string, string> collectionName);

        void SetKeywords(IDictionary<string, string[]> keywords);

        void SetCopyrightNotices(IDictionary<string, string> copyrightNotices);

        void SetIsEditorialUse(bool isEditorialUse);
        void SetHasLicenseTerms(bool hasLicenseTerms);

        void SetLicenseType(string licenseTypeKey);

        void SetLicenseeUuid(string licenseeUuid);
        void SetLicenseeName(string licenseeName);

        void SetLicenseText(IDictionary<string, string> licenseText);

        void SetLicenseOptions(IList<TSyncLicenseOption> licenseOptions);
        void SetLicenseTerms(IList<TSyncLicenseTerm> licenseTerms);

        void SetDownloadConstraints(TSyncDownloadConstraints downloadConstraints);

        void SetReleaseDetails(TSyncReleaseDetails releaseDetails);
    }
}
