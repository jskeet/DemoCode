// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System.Collections.Generic;
using System.Collections.Immutable;

public sealed class Person
{
    public string Name { get; }
    public Address Address { get; }
    public IImmutableList<PhoneNumber> Phones { get; }

    private Person(Builder builder)
    {
        Name = builder.Name;
        Address = builder.Address;
        Phones = Phones.ToImmutableList();
    }

    public sealed class Builder
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<PhoneNumber> Phones { get; } = new List<PhoneNumber>();

        public Person Build()
        {
            return new Person(this);
        }
    }
}
