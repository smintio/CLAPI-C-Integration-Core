using System;
using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoUsageConstraints
    {
        public IList<string> Exclusivities { get; set; }

        public IList<string> AllowedUsages { get; set; }
        public IList<string> RestrictedUsages { get; set; }

        public IList<string> AllowedSizes { get; set; }
        public IList<string> RestrictedSizes { get; set; }

        public IList<string> AllowedPlacements { get; set; }
        public IList<string> RestrictedPlacements { get; set; }

        public IList<string> AllowedDistributions { get; set; }
        public IList<string> RestrictedDistributions { get; set; }

        public IList<string> AllowedGeographies { get; set; }
        public IList<string> RestrictedGeographies { get; set; }

        public IList<string> AllowedVerticals { get; set; }
        public IList<string> RestrictedVerticals { get; set; }

        public IList<string> AllowedLanguages { get; set; }
        public IList<string> RestrictedLanguages { get; set; }

        public int? MaxEditions { get; set; }

        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public DateTimeOffset? ToBeUsedUntil { get; set; }

        public bool? IsEditorialUse { get; set; }
    }
}
