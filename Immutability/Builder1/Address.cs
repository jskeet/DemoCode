// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class Address
{
    public string Street { get; }
    public string City { get; }

    private Address(Builder builder)
    {
        Street = builder.Street;
        City = builder.City;
    }

    public sealed class Builder
    {
        public string Street { get; set; }
        public string City { get; set; }

        public Address Build()
        {
            return new Address(this);
        }
    }
}
