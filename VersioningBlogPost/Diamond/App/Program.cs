using Lib1;
using NodaTime;
using System;

namespace App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Class1.GetFixedDate());
            Console.WriteLine(Class1.GetNow(SystemClock.Instance));
        }
    }
}
