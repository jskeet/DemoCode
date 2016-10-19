// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

class Program
{
    static void Main(string[] args)
    {
        var jon = new Person.Builder
        {
            Name = "Jon",
            Address = new Address.Builder { City = "Reading", Street = "..." }.Build(),
            Phones = {
                new PhoneNumber.Builder { Type = PhoneNumberType.Home, Number = "1234" }.Build(),
                new PhoneNumber.Builder { Type = PhoneNumberType.Mobile, Number = "5678" }.Build(),
            }
        }.Build();
    }
}
