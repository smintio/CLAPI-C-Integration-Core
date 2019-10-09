using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public class SyncTargetCapabilitiesImpl: ISyncTargetCapabilities
    {
        /// <inheritdoc/>
        public SyncTargetCapabilitiesEnum[] Capabilities { get; }

        public SyncTargetCapabilitiesImpl(params SyncTargetCapabilitiesEnum[] capabilities)
        {
            Capabilities = capabilities;
        }

        /// <inheritdoc/>
        public bool IsMultiLanguageSupported()
        {
            return ((IList<SyncTargetCapabilitiesEnum>) Capabilities)
                .Contains(SyncTargetCapabilitiesEnum.MultiLanguageEnum);
        }
    }
}
