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
// [ ] Documentation

namespace Splunk.Client
{
    /// <summary>
    /// Provides a class that represents a collection of Splunk <see cref=
    /// "Configuration"/> files.
    /// </summary>
    public class ConfigurationCollection : EntityCollection<ConfigurationCollection, Configuration>
    {
        internal ConfigurationCollection(Context context, Namespace @namespace)
            : base(context, @namespace, ClassResourceName)
        { }

        public ConfigurationCollection()
        { }

        #region Privates/internals

        internal static readonly ResourceName ClassResourceName = new ResourceName("properties");
        
        #endregion
    }
}
