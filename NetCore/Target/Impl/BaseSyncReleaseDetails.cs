using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public abstract class BaseSyncReleaseDetails
    {
        public abstract void SetModelReleaseState(string modelReleaseStateKey);
        public abstract void SetPropertyReleaseState(string propertyReleaseStateKey);
        public abstract void SetProviderAllowedUseComment(IDictionary<string, string> providerAllowedUseComment);
        public abstract void SetProviderReleaseComment(IDictionary<string, string> providerReleaseComment);
        public abstract void SetProviderUsageConstraints(IDictionary<string, string> providerUsageConstraints);
    }
}
