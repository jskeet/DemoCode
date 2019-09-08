﻿// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Diagnostics.CodeAnalysis;
using VDrumExplorer.Data.Fields;

namespace VDrumExplorer.Data.Json
{
    internal static class Validation
    {
        internal static void ValidateNull<T>(T? value, string name, string becauseOfName)
            where T : class
        {
            if (value is T)
            {
                throw new ModuleSchemaException($"{name} must not be specified because of {becauseOfName}");
            }
        }

        internal static T ValidateNotNull<T>([NotNull] T? value, string name) where T : class =>
            value ?? throw new ModuleSchemaException($"{name} must be specified");

        internal static T ValidateNotNull<T>([NotNull] T? value, string name) where T : struct =>
            value ?? throw new ModuleSchemaException($"{name} must be specified");

        internal static void Validate([DoesNotReturnIf(false)] bool condition, string message)
        {
            if (!condition)
            {
                throw new ModuleSchemaException(message);
            }
        }
    }
}
