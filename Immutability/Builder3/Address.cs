// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class Address
{
    public string Street { get; set; }
    public string City { get; set; }

    public Immutable ToImmutable()
    {
        return new Immutable(this);
    }

    public sealed class Immutable
    {
        public string Street { get; }
        public string City { get; }

        internal Immutable(Address mutable)
        {
            Street = mutable.Street;
            City = mutable.City;
        }
    }
}
