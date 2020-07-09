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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Contracts
{
    /// <summary>A custom dictionary type to provide different translations for values.</summary>
    /// <typeparam name="T">Ensure that this type is serializable to JSON by <see cref="JsonConvert"/></typeparam>
    [Serializable]
    public class TranslatedDictionary<T> : Dictionary<string, T>
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject((object) this);
        }

        public static TranslatedDictionary<T> FromJson(string data)
        {
            return JsonConvert.DeserializeObject<TranslatedDictionary<T>>(data);
        }

        public TranslatedDictionary()
        {
        }

        public TranslatedDictionary(int capacity)
            : base(capacity)
        {
        }

        public TranslatedDictionary(IDictionary<string, T> dictionary)
        {
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, T> keyValuePair in dictionary)
                {
                    Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        /// <summary>Get a translation.</summary>
        ///
        /// <remarks>If no translation for the requested language exists, then the value for key <c>_default_</c>
        /// will be used instead - if it exists.</remarks>
        /// <param name="language">The language to find its translation. Will use
        /// CultureInfo.CurrentCulture.TwoLetterISOLanguageName if not specified.</param>
        /// <returns>The translated string or <c>null</c> if no translation has been found.</returns>
        public T GetTranslation(string language = null)
        {
          string key = language ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

          if (TryGetValue(key, out T valueOfTargetValue))
          {
              return valueOfTargetValue;
          }

          return TryGetValue("en", out T valueOfEnglish) ? valueOfEnglish : default(T);
        }
    }
}
