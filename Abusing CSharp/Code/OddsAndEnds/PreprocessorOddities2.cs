#define FOO
using System;
using System.ComponentModel;

namespace OddsAndEnds
{
    [Description("Verbatim string literals")]
    class PreprocessorOddities2
    {
        static void Main()
        {
#if FOO
            string x = @"FOO is defined
#else
            string x = @";
#else
                         FOO is not defined";
#endif
            Console.WriteLine(x);
        }
    }
}
