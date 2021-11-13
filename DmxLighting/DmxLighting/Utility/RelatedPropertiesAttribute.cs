// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace DmxLighting.Utility
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RelatedPropertiesAttribute : Attribute
    {
        public string[] PropertyNames { get; set; }

        public RelatedPropertiesAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}
