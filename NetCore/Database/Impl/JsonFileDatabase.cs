#region copyright
// MIT License
//
// Copyright (c) 2020 Smint.io GmbH
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
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmintIo.CLAPI.Consumer.Integration.Core.Database.Impl
{
    /// <summary>
    /// Stores and reads data to and from a file in JSON format.
    /// </summary>
    /// <typeparam name="T">The type to store into the file</typeparam>
    public class JsonFileDatabase<T> where T : notnull, new()
    {
        public string FileName { get; private set; }

        public JsonFileDatabase(string storeToFile)
        {
            if (string.IsNullOrEmpty(storeToFile)) throw new ArgumentNullException(nameof(storeToFile));

            FileName = storeToFile;
        }


        /// <summary>
        /// Load the data from the JSON file and deserialize the content.
        /// <remarks>If the files does not exist, an empty new instance is returned.</remarks>
        /// </summary>
        /// <returns>A task to fetch the new or deserialized value as read from the JSON file.</returns>
        public virtual async Task<T> LoadDataAsync()
        {
            if (!File.Exists(FileName))
            {
                return CreateNewInstance();
            }

            return JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(FileName).ConfigureAwait(false));
        }

        /// <summary>
        /// Serialized the provided data to JSON and stores the result string in the file <see cref="FileName"/>
        /// </summary>
        /// <param name="data">The data to serialize</param>
        /// <returns>A task to wait for finishing the storing.</returns>
        public virtual async Task StoreDataAsync(T data)
        {
            await File.WriteAllTextAsync(FileName, JsonConvert.SerializeObject(data)).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a new empty, default instance of the data type <see cref="T"/>.
        /// <remarks>This is used by <see cref="LoadDataAsync"/> to return a value in case the JSON file does not exist.
        /// You should overload this function if your data does not have a default constructor or needs some special data
        /// for default.
        /// </remarks>
        /// </summary>
        /// <returns>A freshly created instance of type <see cref="T"/>.</returns>
        public virtual T CreateNewInstance()
        {
            return new T();
        }
    }
}