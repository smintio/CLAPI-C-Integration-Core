using System.Collections.Generic;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncTarget
    {
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

        void ClearCaches();
    }
}
