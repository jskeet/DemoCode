using System;

namespace Nullability
{
    class ChangingProperty
    {
        string? Name => DateTime.UtcNow.Second == 0 ? null : "not null";

        static void Main()
        {
            var instance = new ChangingProperty();
            instance.PrintNameLength();
        }

        public void PrintNameLength()
        {
            if (Name != null)
            {
                Console.WriteLine(Name.Length);
            }
        }
    }
}
