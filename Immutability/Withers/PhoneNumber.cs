// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class PhoneNumber
{
    public string Number { get; }
    public PhoneNumberType Type { get; }

    public PhoneNumber(string number, PhoneNumberType type)
    {
        Number = number;
        Type = type;
    }

    public PhoneNumber WithNumber(string number)
    {
        return new PhoneNumber(number, Type);
    }

    public PhoneNumber WithType(PhoneNumberType type)
    {
        return new PhoneNumber(Number, type);
    }
}
