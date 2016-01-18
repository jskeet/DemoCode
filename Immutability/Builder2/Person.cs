// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System.Collections.Generic;

public sealed class Person
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public IReadOnlyList<PhoneNumber> Phones { get; }

    private Person()
    {
        Phones = new FreezableList<PhoneNumber>();
    }

    public sealed class Builder
    {
        private Person underlying;

        private Person Underlying => underlying ?? (underlying = new Person());

        public string Name
        {
            get { return Underlying.Name; }
            set { Underlying.Name = value; }
        }

        public Address Address
        {
            get { return Underlying.Address; }
            set { Underlying.Address = value; }
        }

        public IList<PhoneNumber> Phones => (IList<PhoneNumber>) Underlying.Phones;

        public Person Build()
        {
            var ret = Underlying;
            ((FreezableList<PhoneNumber>) ret.Phones).Freeze();
            underlying = null;
            return ret;
        }
    }
}
