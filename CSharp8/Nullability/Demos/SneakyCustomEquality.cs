using System;

namespace Demos
{
    class SneakyCustomEquality
    {
        public int Value { get; }

        public SneakyCustomEquality(int value) => Value = value;

        public static bool operator
            ==(SneakyCustomEquality? lhs, SneakyCustomEquality? rhs) => false;
        public static bool operator
            !=(SneakyCustomEquality? lhs, SneakyCustomEquality? rhs) => true;
        public override int GetHashCode() => 0;
        public override bool Equals(object obj) => false;

        static void Main()
        {
            SneakyCustomEquality? x = null;
            if (x == null)
            {
                Console.WriteLine("X was null");
            }
            else
            {
                // X isn't null, so this should be fine, right? (No: boom!)
                // Compiler is relying on a sensible == implementation.
                // (Not a bad thing, but...)
                Console.WriteLine(x.Value);
            }
        }
    }
}
