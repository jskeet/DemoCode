using System;

namespace WorkingInPreview3
{
    class LocalFunctions1
    {
        static void Main()
        {
            int fib(int n) => n < 2 ? n : fib(n - 1) + fib(n - 2);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(fib(i));
            }
        }
    }
}
