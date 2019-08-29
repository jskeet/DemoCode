using System;

namespace DefaultInterfaceMethods
{
    class StaticMembers
    {
        static void Main()
        {
            IFoo foo1 = new IFoo.FooImpl();
            foo1.M();

            IFoo foo2 = IFoo.DefaultInstance;
            foo2.M();
        }

        public interface IFoo
        {
            static IFoo DefaultInstance = new FooImpl();

            void M();

            // Would usually be a default implementation...
            class FooImpl : IFoo
            {
                public void M() => Console.WriteLine("Invoked");
            }
        }
    }
}
