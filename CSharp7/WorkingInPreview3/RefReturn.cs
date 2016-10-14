using System;

namespace WorkingInPreview3
{
    class RefReturn
    {
        static void Main(string[] args)
        {
            var rng = new Random();
            int a = 0, b = 0;
            for (int i = 0; i < 20; i++)
            {
                GetRandomVariable(rng, ref a, ref b)++;
                Console.WriteLine($"a={a}, b={b}");
            }
        }

        static ref int GetRandomVariable(Random rng, ref int x, ref int y)
        {
            if (rng.NextDouble() >= 0.5)
            {
                return ref x;
            }
            else
            {
                return ref y;
            }
        }
    }
}
