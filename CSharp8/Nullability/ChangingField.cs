using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nullability
{
    class ChangingField
    {
        readonly string? name;

        ChangingField(string? name)
        {
            this.name = name;
            if (this.name != null)
            {
                string? local = null;
                DoSomethingEvil(ref local);
                Console.WriteLine(this.name.Length);
            }
        }

        static void Main()
        {
            new ChangingField("Jon").PrintNameLength();
        }

        public void PrintNameLength()
        {
            if (name != null)
            {
                //DoSomethingEvil(ref name);
                Console.WriteLine(name.Length);
            }
        }

        private void DoSomethingEvil(ref string? x)
        {
            x = null;
        }
    }
}
