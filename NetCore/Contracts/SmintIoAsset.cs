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

using System;
using System.Collections.Generic;
using SmintIo.CLAPI.Consumer.Client.Generated;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoAsset
    {
        public string ContentElementUuid { get; set; }

        public string LicensePurchaseTransactionUuid { get; set; }
        public string CartPurchaseTransactionUuid { get; set; }

        public LicensePurchaseTransactionStateEnum? State { get; set; }

        public string Provider { get; set; }

        public string ContentType { get; set; }

        public IDictionary<string, string> Name { get; set; }
        public IDictionary<string, string> Description { get; set; }

        public IDictionary<string, string[]> Keywords { get; set; }

        public string Category { get; set; }

        public SmintIoReleaseDetails ReleaseDetails { get; set; }

        public IDictionary<string, string> CopyrightNotices { get; set; }

        public string ProjectUuid { get; set; }
        public IDictionary<string, string> ProjectName { get; set; }

        public string CollectionUuid { get; set; }
        public IDictionary<string, string> CollectionName { get; set; }

        public string LicenseeUuid { get; set; }
        public string LicenseeName { get; set; }

        public string LicenseType { get; set; }

        public IDictionary<string, string> LicenseText { get; set; }
        public IList<SmintIoLicenseOptions> LicenseOptions { get; set; }

        public IList<SmintIoLicenseTerm> LicenseTerms { get; set; }
        public SmintIoDownloadConstraints DownloadConstraints { get; set; }

        public bool? IsEditorialUse { get; set; }
        public bool? HasRestrictiveLicenseTerms { get; set; }

        public List<SmintIoBinary> Binaries { get; set; }

        public string SmintIoUrl { get; set; }
        
        public DateTimeOffset PurchasedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}

