// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System;

public sealed class Mutable
{
    public int Value { get; set; }
}

public class UsageOfMutable
{
    public static void Main()
    {
        var m = new Mutable();
        m.Value = 20;
        Console.WriteLine(m.Value); // 20
        m.Value = 30;
        Console.WriteLine(m.Value); // 30
    }
}