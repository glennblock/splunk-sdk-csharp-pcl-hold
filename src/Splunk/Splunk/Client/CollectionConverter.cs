﻿/*
 * Copyright 2014 Splunk, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"): you may
 * not use this file except in compliance with the License. You may obtain
 * a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

// TODO:
// [ ] Contracts
// [O] Documentation

namespace Splunk.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides a converter to convert an <see cref="IEnumerable"/> object to a
    /// collection.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of the values in the collection.
    /// </typeparam>
    /// <typeparam name="TCollection">
    /// The type of the collection.
    /// </typeparam>
    /// <typeparam name="TConverter">
    /// The type of value converter to convert items enumerated by the <see 
    /// cref="IEnumerable"/> object.
    /// </typeparam>
    sealed class CollectionConverter<TValue, TCollection, TConverter> : ValueConverter<TCollection> 
        where TCollection : ICollection<TValue>, new()
        where TConverter : ValueConverter<TValue>, new()
    {
        static CollectionConverter()
        {
            Instance = new CollectionConverter<TValue, TCollection, TConverter>();
            ValueConverter = new TConverter();
        }

        public static CollectionConverter<TValue, TCollection, TConverter> Instance
        { get; private set; }

        public override TCollection Convert(object input)
        {
            var list = input as IEnumerable<object>;

            if (list == null)
            {
                throw new InvalidDataException(string.Format("Expected {0}: {1}", TypeName, input)); // TODO: improved diagnostics
            }

            var collection = new TCollection();

            foreach (var value in list)
            {
                collection.Add(ValueConverter.Convert(value));
            }

            return collection;
        }

        static readonly TConverter ValueConverter;
    }
}
