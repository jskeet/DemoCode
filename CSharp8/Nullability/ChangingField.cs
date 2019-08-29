using System;

namespace Nullability
{
    class ChangingField
    {
        string? name;

        ChangingField(string? name)
        {
            this.name = name;
        }

        static void Main()
        {
            new ChangingField("Jon").PrintNameLength();
        }

        public void PrintNameLength()
        {
            if (name != null)
            {
                DoSomethingEvil();
                Console.WriteLine(name.Length);
            }
        }

        private void DoSomethingEvil()
        {
            name = null;
        }
    }
}
