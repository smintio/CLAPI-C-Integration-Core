using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoUsageConstraints
    {
        public bool? EffectiveIsExclusive { get; set; }

        public IList<string> EffectiveAllowedUsages { get; set; }
        public IList<string> EffectiveRestrictedUsages { get; set; }

        public IList<string> EffectiveAllowedSizes { get; set; }
        public IList<string> EffectiveRestrictedSizes { get; set; }

        public IList<string> EffectiveAllowedPlacements { get; set; }
        public IList<string> EffectiveRestrictedPlacements { get; set; }

        public IList<string> EffectiveAllowedDistributions { get; set; }
        public IList<string> EffectiveRestrictedDistributions { get; set; }

        public IList<string> EffectiveAllowedGeographies { get; set; }
        public IList<string> EffectiveRestrictedGeographies { get; set; }

        public IList<string> EffectiveAllowedVerticals { get; set; }
        public IList<string> EffectiveRestrictedVerticals { get; set; }

        public DateTimeOffset? EffectiveValidFrom { get; set; }
        public DateTimeOffset? EffectiveValidUntil { get; set; }
        public DateTimeOffset? EffectiveToBeUsedUntil { get; set; }

        public bool? EffectiveIsEditorialUse { get; set; }
    }
}
