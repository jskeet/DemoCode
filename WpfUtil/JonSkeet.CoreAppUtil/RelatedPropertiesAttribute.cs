// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace JonSkeet.CoreAppUtil;

/// <summary>
/// Indicates a set of properties related to the one that the attribute is applied to.
/// When the property the attribute is applied is changed via <see cref="ViewModelBase"/> methods,
/// notifications are raised for the related properties too.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RelatedPropertiesAttribute(params string[] propertyNames) : Attribute
{
    public string[] PropertyNames { get; set; } = propertyNames;
}
