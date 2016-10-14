// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public class UnsealedSemiImmutable
{
    public int Value { get; }

    public UnsealedSemiImmutable(int value)
    {
        Value = value;
    }
}

public class DerivedMutable : UnsealedSemiImmutable
{
    public int OtherValue { get; set; }

    public DerivedMutable(int value) : base(value)
    {
    }
}

public class ExampleOfUnsealedImmutable
{
    private readonly UnsealedSemiImmutable foo;

    public ExampleOfUnsealedImmutable(UnsealedSemiImmutable foo)
    {
        this.foo = foo;
    }

    public static void Main()
    {
        var naughty = new DerivedMutable(1);
        var example = new ExampleOfUnsealedImmutable(naughty);
        naughty.OtherValue = 10;
    }
}
