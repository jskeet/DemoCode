using System;

namespace DefaultInterfaceMethods
{
    public class InternalAccess
    {
        static void Main()
        {
            IFoo foo = new Foo1();
            foo.InternalOnly();
        }

        public interface IFoo
        {
            internal void InternalOnly();
        }

        public class Foo1 : IFoo
        {
            // Can't just write internal void InternalOnly() => ...
            // Has to be explicit.
            void IFoo.InternalOnly() => Console.WriteLine("Can't call this externally");
        }
    }
}
