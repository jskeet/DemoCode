using System;
using System.IO;
using System.Reflection;

namespace MultiVersionLoading
{
    class Program
    {
        static void Main(string[] args)
        {
            string path131 = Path.GetFullPath("NodaTime-1.3.1.dll");
            Assembly nodaTime131 = Assembly.LoadFile(path131);
            dynamic clock131 = nodaTime131
                .GetType("NodaTime.SystemClock")
                // Instance is a field 1.x
                .GetField("Instance")
                .GetValue(null);
            Console.WriteLine(clock131.Now);

            string path200 = Path.GetFullPath("NodaTime-2.0.0.dll");
            Assembly nodaTime200 = Assembly.LoadFile(path200);
            dynamic clock200 = nodaTime200
                .GetType("NodaTime.SystemClock")
                // Instance is a property in 2.x
                .GetProperty("Instance")
                .GetValue(null);
            Console.WriteLine(clock200.GetCurrentInstant());
        }
    }
}
