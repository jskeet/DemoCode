// Copyright 2024 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace JonSkeet.WpfUtil;

/// <summary>
/// Specifies a method to invoke when this property has changed (after
/// raising the property changed notification).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ReactionMethodAttribute : Attribute
{
    public string MethodName { get; set; }

    public ReactionMethodAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
