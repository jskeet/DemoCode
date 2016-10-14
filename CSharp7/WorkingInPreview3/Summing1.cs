using System;
using System.Collections.Generic;

namespace WorkingInPreview3
{
    class Summing1
    {
        static void Main()
        {
            Console.WriteLine(SumAndCount(new[] { 5, 3, 1 }));
        }

        static (int sum, int count) SumAndCount(IEnumerable<int> values)
        {
            int sum = 0;
            int count = 0;
            foreach (var value in values)
            {
                sum += value;
                count++;
            }
            return (sum, count);
        }
    }
}
