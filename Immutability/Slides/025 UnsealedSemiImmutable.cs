// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System;

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

    public override string ToString() => OtherValue.ToString();
}

public class ExampleOfUnsealedImmutable
{
    public static void Main()
    {
        var naughty = new DerivedMutable(1);

        UnsealedSemiImmutable isThisImmutable = naughty;
        Console.WriteLine(isThisImmutable); // 0
        naughty.OtherValue = 10;
        Console.WriteLine(isThisImmutable); // 10! State has changed.
    }
}
