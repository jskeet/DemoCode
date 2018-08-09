using System;

namespace Demos
{
    class DeconstructionInPatterns
    {
        static void Main()
        {
            ShowFact(new DateTime(2016, 2, 29));
            ShowFact(DateTime.Today);
            ShowFact(new DateTime(2018, 6, 19));
            ShowFact(new DateTime(2015, 5, 1));

            void ShowFact(DateTime date) => Console.WriteLine(GetInterestingFact(date));
        }

        static string GetInterestingFact(DateTime date) => date switch
        {
            (_, _, 1) => $"{date:d} is the first of the month",
            (_, 6, 19) => $"{date:d} is Jon's birthday",
            (int year, 2, 29) => $"{year} is a leap year",
            _ => $"{date:d} is a boring date"
        };
    }
}
