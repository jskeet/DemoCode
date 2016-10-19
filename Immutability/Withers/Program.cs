// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System;
using System.Collections.Immutable;

class Program
{
    static void Main(string[] args)
    {
        var jon = new Person(
            name: "Foo",
            address: null, // Do this later
            phones: new[] { new PhoneNumber("1235", PhoneNumberType.Home) }.ToImmutableList());
        var later = jon.WithAddress(new Address("School Road", "Reading"));
    }
}
