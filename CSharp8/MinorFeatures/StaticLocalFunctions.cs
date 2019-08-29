using System;

namespace MinorFeatures
{
    class StaticLocalFunctions
    {
        int field;

        public StaticLocalFunctions()
        {
            // Just to avoid the warning...
            field = 10;
        }

        static void Main()
        {
            new StaticLocalFunctions().Method();
        }

        void Method()
        {
            int capture = 10;
            Foo();
            Bar();

            void Foo() => Console.WriteLine($"I can access {field} and {capture}");
            static void Bar() => Console.WriteLine("I can't access field or capture");
        }
    }
}
