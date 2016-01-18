// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

public sealed class Address(
    string street: Street,
    string city: City);

public sealed class Person(
    string name: Name,
    Address address: Address,
    IImmutableList<PhoneNumber> phones: Phones);

public sealed class PhoneNumber(
    string number: Number,
    PhoneNumberType type: Type);
