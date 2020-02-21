using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultInterfaceMethods
{
    class BasicDemo
    {
        static void Main()
        {
            IFoo foo1 = new Foo1();
            foo1.CallMTwice();

            IFoo foo2 = new Foo2();
            foo2.CallMTwice();
        }

        interface IFoo
        {
            void M();

            void CallMTwice()
            {
                Console.WriteLine("Calling M twice in default interface implementation");
                M();
                M();
                Console.WriteLine("Done");
            }
        }

        class Foo1 : IFoo
        {
            public void M() => Console.WriteLine("Foo1.M");
        }

        class Foo2 : IFoo
        {
            public void M() => Console.WriteLine("Foo2.M");

            public void CallMTwice()
            {
                Console.WriteLine("Calling M twice in Foo2");
                M();
                M();
                Console.WriteLine("Done");
            }
        }
    }
}
