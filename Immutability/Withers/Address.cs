// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class Address
{
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city)
    {
        Street = street;
        City = city;
    }

    public Address WithStreet(string street)
    {
        return new Address(street, City);
    }

    public Address WithCity(string city)
    {
        return new Address(Street, city);
    }
}
