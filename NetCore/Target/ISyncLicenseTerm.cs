using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncLicenseTerm
    {
        void SetSequenceNumber(int sequenceNumber);

        void SetName(IDictionary<string, string> name);

        void SetExclusivities(IList<string> exclusivityKeys);

        void SetAllowedUsages(IList<string> usageKeys);
        void SetRestrictedUsages(IList<string> usageKeys);

        void SetAllowedSizes(IList<string> sizeKeys);
        void SetRestrictedSizes(IList<string> sizeKeys);

        void SetAllowedPlacements(IList<string> placementKeys);
        void SetRestrictedPlacements(IList<string> placementKeys);

        void SetAllowedDistributions(IList<string> distributionKeys);
        void SetRestrictedDistributions(IList<string> distributionKeys);

        void SetAllowedGeographies(IList<string> geographyKeys);
        void SetRestrictedGeographies(IList<string> geographyKeys);

        void SetAllowedIndustries(IList<string> industryKeys);
        void SetRestrictedIndustries(IList<string> industryKeys);

        void SetAllowedLanguages(IList<string> languageKeys);
        void SetRestrictedLanguages(IList<string> languageKeys);

        void SetUsageLimits(IList<string> usageLimitKeys);

        void SetValidFrom(DateTimeOffset validFrom);
        void SetValidUntil(DateTimeOffset validUntil);
        void SetToBeUsedUntil(DateTimeOffset toBeUsedUntil);

        void SetIsEditorialUse(bool isEditorialUse);
    }
}
