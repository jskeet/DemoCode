// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }

    private Address()
    {
    }

    public sealed class Builder
    {
        private Address underlying;

        private Address Underlying => underlying ?? (underlying = new Address());

        public string City
        {
            get { return Underlying.City; }
            set { Underlying.City = value; }
        }

        public string Street
        {
            get { return Underlying.Street; }
            set { Underlying.Street = value; }
        }

        public Address Build()
        {
            var ret = Underlying;
            underlying = null;
            return ret;
        }
    }
}
