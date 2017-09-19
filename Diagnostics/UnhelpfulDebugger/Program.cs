using System;

namespace UnhelpfulDebugger
{
    class Program
    {
        static void Main()
        {
            string awkward1 = "Foo\\Bar";
            string awkward2 = "FindEle‌​ment";
            double awkward3 = 4.9999999999999995d;

            Console.WriteLine(awkward1);
            PrintString(awkward1);

            Console.WriteLine(awkward2);
            PrintString(awkward2);

            Console.WriteLine(awkward3);
            PrintDouble(awkward3);
        }

        static void PrintString(string text)
        {
            Console.WriteLine($"Text: '{text}'");
            Console.WriteLine($"Length: {text.Length}");
            for (int i = 0; i < text.Length; i++)
            {
                Console.WriteLine($"{i,2} U+{(int) text[i]:x4} '{text[i]}'");
            }
        }

        static void PrintDouble(double value) =>
            Console.WriteLine(DoubleConverter.ToExactString(value));
    }
}
