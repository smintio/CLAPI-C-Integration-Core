using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoReleaseDetails
    {
        public string ModelReleaseState { get; set; }
        public string PropertyReleaseState { get; set; }

        public IDictionary<string, string> ProviderAllowedUseComment { get; set; }

        public IDictionary<string, string> ProviderReleaseComment { get; set; }

        public IDictionary<string, string> ProviderUsageConstraints { get; set; }
    }
}
