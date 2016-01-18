// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class PhoneNumber
{
    public string Number { get; set; }
    public PhoneNumberType Type { get; set; }

    public Immutable ToImmutable()
    {
        return new Immutable(this);
    }

    public sealed class Immutable
    {
        public string Number { get; }
        public PhoneNumberType Type { get; }

        internal Immutable(PhoneNumber mutable)
        {
            Number = mutable.Number;
            Type = mutable.Type;
        }
    }
}
