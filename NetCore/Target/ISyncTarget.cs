using System.Collections.Generic;
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncTarget
    {
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
        /// <remarks>After all synchronisation took place, some clean-up might be necessary, besides <c>ClearCaches</c>
        /// (see <see cref="ClearCaches"/>). The task returned by this hook is executed after all synchronisation has
        /// ended - no matter whether successfully or not - but <i>before</i> <see cref="ClearCaches"/> is called. 
        /// So the running task still has access to all the created caches.
        /// </remarks>
         /// <exception cref="Exception">Any such exception is being caught and ignored. The task does not need to take
        /// special care.</exception>
        Task AfterSyncAsync();


        void ClearCaches();
    }
}
