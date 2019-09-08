// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public static class FieldExtensions
    {
        public static bool IsEnabled(this IField field, FixedContainer context, ModuleData data) =>
            field.Condition?.IsEnabled(field, context, data) ?? true;
    }
}
