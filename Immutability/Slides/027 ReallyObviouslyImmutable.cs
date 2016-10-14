// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class ReallyObviouslyImmutable
{
    public string Name { get; }

    public int Value { get; }

    public ReallyObviouslyImmutable(string name, int value)
    {
        this.Name = name;
        this.Value = value;
    }
}