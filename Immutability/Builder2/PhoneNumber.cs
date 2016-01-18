// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class PhoneNumber
{
    public string Number { get; private set; }
    public PhoneNumberType Type { get; private set; }

    private PhoneNumber()
    {
    }

    public sealed class Builder
    {
        private PhoneNumber underlying;

        private PhoneNumber Underlying => underlying ?? (underlying = new PhoneNumber());

        public string Number
        {
            get { return Underlying.Number; }
            set { Underlying.Number = value; }
        }

        public PhoneNumberType Type
        {
            get { return Underlying.Type; }
            set { Underlying.Type = value; }
        }

        public PhoneNumber Build()
        {
            var ret = Underlying;
            underlying = null;
            return ret;
        }
    }
}
