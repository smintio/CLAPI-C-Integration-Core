using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public interface ISyncReleaseDetails
    {
        void SetModelReleaseState(string modelReleaseStateKey);
        void SetPropertyReleaseState(string propertyReleaseStateKey);
        void SetProviderAllowedUseComment(IDictionary<string, string> providerAllowedUseComment);
        void SetProviderReleaseComment(IDictionary<string, string> providerReleaseComment);
        void SetProviderUsageConstraints(IDictionary<string, string> providerUsageConstraints);
    }
}
