// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class ObservablyImmutable
{
    private int cachedHash;

    public string Name { get; }

    public ObservablyImmutable(string name)
    {
        this.Name = name;
    }

    public override bool Equals(object obj)
    {
        var other = obj as ObservablyImmutable;
        if (other == null)
        {
            return false;
        }
        return other.Name == Name;
    }

    public override int GetHashCode()
    {
        if (cachedHash == 0)
        {
            cachedHash = Name.GetHashCode();
        }
        return cachedHash;
    }
}
