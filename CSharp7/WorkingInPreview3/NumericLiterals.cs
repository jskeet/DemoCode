using System;

namespace WorkingInPreview3
{
    class NumericLiterals
    {
        static void Main()
        {
            int alternateBits = 0b10101010;
            int argb = 0x7f_ff_00_77;
            int million = 1_000_000;

            Console.WriteLine(alternateBits);
            Console.WriteLine(argb);
            Console.WriteLine(million);
        }
    }
}
