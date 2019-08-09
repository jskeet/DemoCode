// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace VDrumExplorer.Data.Fields
{
    public interface IField
    {
        ModuleSchema Schema { get; }
        string Description { get; }
        FieldPath Path { get; }
        ModuleAddress Address { get; }
        int Size { get; }
        FieldCondition? Condition { get; }
    }
}
