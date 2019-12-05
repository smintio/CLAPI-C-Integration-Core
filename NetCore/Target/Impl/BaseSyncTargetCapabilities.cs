#region copyright
// MIT License
//
// Copyright (c) 2019 Smint.io GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice (including the next paragraph) shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// SPDX-License-Identifier: MIT
#endregion

using System.Collections.Generic;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl
{
    public enum SyncTargetCapabilitiesEnum
    {
        MultiLanguageEnum,
        CompoundAssetsEnum,
        BinaryUpdatesEnum
    }

    public class BaseSyncTargetCapabilities
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
        public SyncTargetCapabilitiesEnum[] Capabilities { get; }

        public BaseSyncTargetCapabilities(params SyncTargetCapabilitiesEnum[] capabilities)
        {
            Capabilities = capabilities;
        }

        /// <summary>
        /// Indicates whether multi languages are supported by this sync target implementation.
        /// </summary>
        ///
        /// <remarks>The value should be calculated based on the provided <see cref="Capabilities"/>.
        /// </remarks>
        public bool IsMultiLanguageSupported()
        {
            return ((IList<SyncTargetCapabilitiesEnum>) Capabilities)
                .Contains(SyncTargetCapabilitiesEnum.MultiLanguageEnum);
        }

        /// <summary>
        /// Indicates whether compound assets are supported by this sync target implementation.
        /// </summary>
        ///
        /// <remarks>The value should be calculated based on the provided <see cref="Capabilities"/>.
        /// </remarks>
        public bool IsCompoundAssetsSupported()
        {
            return ((IList<SyncTargetCapabilitiesEnum>)Capabilities)
                .Contains(SyncTargetCapabilitiesEnum.CompoundAssetsEnum);
        }

        /// <summary>
        /// Indicates whether binary updates are supported by this sync target implementation.
        /// </summary>
        ///
        /// <remarks>The value should be calculated based on the provided <see cref="Capabilities"/>.
        /// </remarks>
        public bool IsBinaryUpdatesSupported()
        {
            return ((IList<SyncTargetCapabilitiesEnum>)Capabilities)
                .Contains(SyncTargetCapabilitiesEnum.BinaryUpdatesEnum);
        }
    }
}
