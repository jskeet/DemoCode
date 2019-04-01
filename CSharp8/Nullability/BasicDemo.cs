#nullable disable
// All code is available at:
// https://github.com/jskeet/DemoCode

using System;

namespace Nullability
{
    class Person
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string MiddleName { get; }

        public Person(string firstName, string lastName, string middleName)
        {
            // TODO: Validate arguments
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
        }
    }

    class BasicDemo
    {
        static void Main()
        {
            var jon = new Person("Jon", "Skeet", "Leslie");
            var miguel = new Person("Miguel", "de Icaza", null);
            PrintNameLengths(jon);
            PrintNameLengths(miguel);
        }

        static void PrintNameLengths(Person person)
        {
            string first = person.FirstName;
            string middle = person.MiddleName;
            string last = person.LastName;

            Console.WriteLine("First={0}; Last={1}; Middle={2}",
                first.Length,
                last.Length,
                middle.Length);
        }
    }
}
