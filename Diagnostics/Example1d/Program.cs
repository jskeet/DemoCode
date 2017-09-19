using System;
using System.Globalization;

namespace Example1d
{
    class Program
    {
        static void Main()
        {
            string date = "05/06/2017";
            DateTime dateTime = DateTime.Parse(date);
            Console.WriteLine(dateTime);
            Console.WriteLine(dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
