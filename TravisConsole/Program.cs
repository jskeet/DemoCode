using System;

class Program
{
    static void Main(string[] args)
    {
        int count = int.Parse(args[0]);
        for (int i = 1; i <= count; i++)
        {
            Console.WriteLine($"Line {i}");
        }
    }
}
