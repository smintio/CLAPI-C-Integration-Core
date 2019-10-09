using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public class SyncTargetCapabilitiesImpl: ISyncTargetCapabilities
    {
        private readonly SyncTargetCapabilitiesEnum[] _capabilities;

        public SyncTargetCapabilitiesImpl(SyncTargetCapabilitiesEnum[] capabilities)
        {
            _capabilities = capabilities;
        }

        /// <inheritdoc/>
        public SyncTargetCapabilitiesEnum[] Capabilities => _capabilities;

        /// <inheritdoc/>
        public bool IsMultiLanguageSupported()
        {
            return ((IList<SyncTargetCapabilitiesEnum>) _capabilities)
                .Contains(SyncTargetCapabilitiesEnum.MultiLanguageEnum);
        }
    }
}
