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

using SmintIo.CLAPI.Consumer.Integration.Core.Exceptions;
using System;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Models
{
    /// <summary>
    /// A data model to hold some data needed to synchronize assets and meta data between Smint.IO and external systems.
    /// </summary>
    /// <remarks>This data is needed to sync and its existence is checked:
    /// <para>For authentication via OAuth with Smint.io:
    ///   <list type="bullet">
    ///     <item><see cref="ClientId"/> ... the user ID within Smint.io</item>
    ///     <item><see cref="ClientSecret"/> ... the OAuth secret for the client to authenticate with Smint.io</item>
    ///     <item><see cref="TenantId"/> ... the ID of the client's tenant within Smint.io</item>
    ///     <item><see cref="RedirectUri"/> ... the URI to use as OAuth redirection when authenticating with Smint.io
    ///     </item>
    ///   </list>
    /// </para>
    /// <para>For a synchronization with Smint.io:
    ///   <list type="bullet">
    ///     <item><see cref="TenantId"/> ... the ID of the client's tenant to sync its assets</item>
    ///     <item><see cref="ImportLanguages"/> ... which language to sync for meta data of assets</item>
    ///   </list>
    /// </para>
    /// </remarks>
    public class SmintIoSettingsDatabaseModel
    {
        public string TenantId { get; set; }
        public int? ChannelId { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public string[] ImportLanguages { get; set; }

        internal void ValidateForAuthenticator()
        {
            if (string.IsNullOrEmpty(TenantId)) throw new ArgumentNullException(nameof(TenantId));
            if (string.IsNullOrEmpty(ClientId)) throw new ArgumentNullException(nameof(ClientId));
            if (string.IsNullOrEmpty(ClientSecret)) throw new ArgumentNullException(nameof(ClientSecret));
            if (string.IsNullOrEmpty(RedirectUri)) throw new ArgumentNullException(nameof(RedirectUri));
        }

        internal void ValidateForSync()
        {
            if (string.IsNullOrEmpty(TenantId)) throw new ArgumentNullException(nameof(TenantId));
            if (ImportLanguages == null || ImportLanguages.Length == 0) throw new ArgumentException("Import languages missing", nameof(ImportLanguages));
        }

        internal void ValidateForPusher()
        {
            ValidateForSync();

            if (ChannelId == null) throw new ArgumentNullException(nameof(ChannelId));
            if (ChannelId <= 0) throw new ArgumentException("Must not be equal or smaller than 0", nameof(ChannelId));
        }
    }
}
