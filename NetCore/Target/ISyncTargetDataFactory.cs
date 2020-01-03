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
using System.Threading.Tasks;
using SmintIo.CLAPI.Consumer.Integration.Core.Contracts;
using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using SmintIo.CLAPI.Consumer.Integration.Core.Target.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Target;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Target
{
    /// <summary>
    /// Factory to create target specific data instances to write meta data to.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Meta data is structured into various classes (interfaces). Target specific instances are used to store these meta
    /// data. Implementors of this factory are delivering such instances, related to the DAM target.
    /// </para>
    /// 
    /// </remarks>
    public interface ISyncTargetDataFactory<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncAsset : BaseSyncAsset<TSyncAsset, TSyncLicenseTerm, TSyncReleaseDetails, TSyncDownloadConstraints>
        where TSyncLicenseTerm : ISyncLicenseTerm
        where TSyncReleaseDetails : ISyncReleaseDetails
        where TSyncDownloadConstraints : ISyncDownloadConstraints
    {

        /// <summary>
        /// Factory function to create a new instance representing target's data structure for a <em>Binary Assets</em>.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// The newly created instance MUST NOT be stored automatically on the sync target prior to returning it.
        /// For storing this instance, it will be passed to this sync target via
        /// <see cref="ISyncTarget{TSyncAsset,TSyncLicenseTerm,TSyncReleaseDetails,TSyncDownloadConstraints}.ImportNewTargetAssetsAsync" />.
        /// </para>
        /// 
        /// </remarks>
        /// <returns>must return a fresh instance for every call and must not ever return <c>null</c>.</returns>
        /// @see BaseSyncAsset
        TSyncAsset CreateSyncBinaryAsset();

        /// <summary>
        /// Factory function to create a new instance representing target's data structure for <em>Compound Assets</em>.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// The newly created instance MUST NOT be stored automatically on the sync target prior to returning it. For
        /// storing this instance, it will be passed to this sync target via
        /// <see cref="ISyncTarget{TSyncAsset,TSyncLicenseTerm,TSyncReleaseDetails,TSyncDownloadConstraints}.ImportNewTargetCompoundAssetsAsync" />.
        /// </para>
        /// 
        /// </remarks>
        /// <returns>must return a fresh instance for every call and must not ever return <c>null</c>.</returns>
        /// @see BaseSyncAsset
        TSyncAsset CreateSyncCompoundAsset();

        TSyncLicenseTerm CreateSyncLicenseTerm();
        TSyncReleaseDetails CreateSyncReleaseDetails();
        TSyncDownloadConstraints CreateSyncDownloadConstraints();
    }
}


