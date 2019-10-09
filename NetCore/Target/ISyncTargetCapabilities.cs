﻿namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    public enum SyncTargetCapabilitiesEnum
    {
            MultiLanguageEnum
    }

    public interface ISyncTargetCapabilities
    {
        /// <summary>
        /// Provides a list if capabilities enumeration values indicating supported features of the sync target.
        /// </summary>
        ///
        /// <remarks>Every synchronisation target is different and relies on the capabilities of the underlying
        /// DAM infrastructure. The list of enumerations provide a means to query the capabilities and restrictions
        /// of this sync target.
        ///
        /// <para>The support flag <see cref="SyncTargetCapabilitiesEnum.MultiLanguageEnum"/> indicating
        /// multi-language support is evaluated very early in the synchronisation process. Right at the start the
        /// sync configuration is checked, whether it matches the capabilities regarding multi-language support.
        /// If they do not comply, the synchronisation task is not started but aborted immediately.
        /// </para>
        /// </remarks>
        /// <returns>The array of capability enumeration indicates all features the sync target is capable of
        /// supporting. If none of the enumerable features are supported, either return <c>null</c> value</returns>
        SyncTargetCapabilitiesEnum[] Capabilities { get; }


        /// <summary>
        /// Indicates whether multi languages are supported by this sync target implementation.
        /// </summary>
        ///
        /// <remarks>The value should be calculated based on the provided <see cref="Capabilities"/>.
        /// </remarks>
        bool IsMultiLanguageSupported();
    }
}
