using System;
using System.Diagnostics;

namespace Example3c
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Time(1);
            Time(1000000);
        }

        private static void Time(int iterations)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                "abcfeg".EndsWith("123");
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
        }
    }
}