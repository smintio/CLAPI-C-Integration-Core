using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public abstract class SyncLicenseTermImpl : ISyncLicenseTerm
    {
        public abstract void SetName(IDictionary<string, string> name);

        public abstract void SetSequenceNumber(int sequenceNumber);

        public abstract void SetExclusivities(IList<string> exclusivityKeys);

        public abstract void SetAllowedUsages(IList<string> usageKeys);
        public abstract void SetRestrictedUsages(IList<string> usageKeys);
        public abstract void SetAllowedSizes(IList<string> sizeKeys);
        public abstract void SetRestrictedSizes(IList<string> sizeKeys);
        public abstract void SetAllowedPlacements(IList<string> placementKeys);
        public abstract void SetRestrictedPlacements(IList<string> placementKeys);
        public abstract void SetAllowedDistributions(IList<string> distributionKeys);
        public abstract void SetRestrictedDistributions(IList<string> distributionKeys);
        public abstract void SetAllowedGeographies(IList<string> geographyKeys);
        public abstract void SetRestrictedGeographies(IList<string> geographyKeys);
        public abstract void SetAllowedIndustries(IList<string> industryKeys);
        public abstract void SetRestrictedIndustries(IList<string> industryKeys);
        public abstract void SetAllowedLanguages(IList<string> languageKeys);
        public abstract void SetRestrictedLanguages(IList<string> languageKeys);
        public abstract void SetUsageLimits(IList<string> usageLimitKeys);

        public abstract void SetValidFrom(DateTimeOffset validFrom);
        public abstract void SetValidUntil(DateTimeOffset validUntil);
        public abstract void SetToBeUsedUntil(DateTimeOffset toBeUsedUntil);

        public abstract void SetIsEditorialUse(bool isEditorialUse);
    }
}
