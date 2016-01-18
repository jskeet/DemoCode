// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System.Collections.Generic;
using System.Collections.Immutable;

public sealed class Person
{
    public string Name { get; }
    public Address Address { get; }
    public IImmutableList<PhoneNumber> Phones { get; }

    public Person(string name, Address address, IImmutableList<PhoneNumber> phones)
    {
        Name = name;
        Address = address;
        Phones = phones;
    }

    public Person WithName(string name)
    {
        return new Person(name, Address, Phones);
    }

    public Person WithAddress(Address address)
    {
        return new Person(Name, address, Phones);
    }

    public Person WithPhones(IImmutableList<PhoneNumber> phones)
    {
        return new Person(Name, Address, phones);
    }
}
