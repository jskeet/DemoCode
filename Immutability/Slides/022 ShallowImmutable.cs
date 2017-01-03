// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System;
using System.Text;

public sealed class ShallowImmutable
{
    // Fine
    public int Value { get; }
    // Mutable
    public StringBuilder NameBuilder { get; }
    // See also: collections...

    public ShallowImmutable(int value, StringBuilder nameBuilder)
    {
        Value = value;
        NameBuilder = nameBuilder;
    }
}

public class UsageOfShallowMutable
{
    public static void Main()
    {
        StringBuilder x = new StringBuilder();
        var m = new ShallowImmutable(10, x);
        x.Append("foo");
        Console.WriteLine(m.NameBuilder); // foo
        m.NameBuilder.Append("bar");
        Console.WriteLine(m.NameBuilder); // foobar
    }
}