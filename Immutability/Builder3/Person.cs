// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed class Person
{
    public string Name { get; set; }
    public Address Address { get; set; }
    public List<PhoneNumber> Phones { get; set; }

    public Immutable ToImmutable()
    {
        return new Immutable(this);
    }

    public sealed class Immutable
    {
        public string Name { get; }
        public Address.Immutable Address { get; }
        public IImmutableList<PhoneNumber.Immutable> Phones { get; }

        internal Immutable(Person mutable)
        {
            Name = mutable.Name;
            Address = mutable.Address.ToImmutable();
            Phones = mutable.Phones.Select(x => x.ToImmutable()).ToImmutableList();
        }
    }
}
