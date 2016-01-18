// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class PhoneNumber
{
    public string Number { get; }
    public PhoneNumberType Type { get; }

    private PhoneNumber(Builder builder)
    {
        Number = builder.Number;
        Type = builder.Type;
    }

    public sealed class Builder
    {
        public string Number { get; set; }
        public PhoneNumberType Type { get; set; }

        public PhoneNumber Build()
        {
            return new PhoneNumber(this);
        }
    }
}
