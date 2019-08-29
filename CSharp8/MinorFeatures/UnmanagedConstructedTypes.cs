using System;

namespace MinorFeatures
{
    class UnmanagedConstructedTypes
    {
        static void Main()
        {
            var foo = new Foo<Foo<int>>(new Foo<int>(0));
            Console.WriteLine(foo.Value.Value);
        }

        public struct Foo<T> where T : unmanaged
        {
            public T Value { get; }

            public Foo(T value) => Value = value;
        }
    }
}
