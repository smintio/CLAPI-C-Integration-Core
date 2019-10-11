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

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    public class SmintIoLicenseTerm
    {
        public int? SequenceNumber { get; set; }

        public IDictionary<string, string> Name { get; set; }

        public IList<string> Exclusivities { get; set; }

        public IList<string> AllowedUsages { get; set; }
        public IList<string> RestrictedUsages { get; set; }

        public IList<string> AllowedSizes { get; set; }
        public IList<string> RestrictedSizes { get; set; }

        public IList<string> AllowedPlacements { get; set; }
        public IList<string> RestrictedPlacements { get; set; }

        public IList<string> AllowedDistributions { get; set; }
        public IList<string> RestrictedDistributions { get; set; }

        public IList<string> AllowedGeographies { get; set; }
        public IList<string> RestrictedGeographies { get; set; }

        public IList<string> AllowedIndustries { get; set; }
        public IList<string> RestrictedIndustries { get; set; }

        public IList<string> AllowedLanguages { get; set; }
        public IList<string> RestrictedLanguages { get; set; }

        public IList<string> UsageLimits { get; set; }

        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public DateTimeOffset? ToBeUsedUntil { get; set; }

        public bool? IsEditorialUse { get; set; }
    }
}
