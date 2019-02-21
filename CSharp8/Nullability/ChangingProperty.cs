using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nullability
{
    class ChangingProperty
    {
        string? Name { get; set; }

        public void PrintNameLength()
        {
            if (Name != null)
            {
                Console.WriteLine(Name.Length);
            }
        }
    }
}
