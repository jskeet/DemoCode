using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
{
    struct BigStruct
    {
        internal int x, y, z;
        internal long a, b, c;
    }

    class RefLocal
    {
        static void Main()
        {
            BigStruct[] array = new BigStruct[1000];

            Console.WriteLine(array[1].a); // 0
            for (int i = 0; i < array.Length; i++)
            {
                ref BigStruct refLocal = ref array[i];
                if (refLocal.x < i)
                {
                    refLocal.a++;
                }
            }
            Console.WriteLine(array[1].a); // 1
        }
    }
}
