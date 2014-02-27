#define FOO
using System;
using System.ComponentModel;

namespace OddsAndEnds
{
    [Description("Multi-line comments")]
    class PreprocessorOddities1
    {
        static void Main()
        {
#if FOO
            Console.WriteLine("FOO is defined");
            /* This is a comment
#else
            Console.WriteLine("FOO is not defined");
            /*/
#else
            /*/
            Console.WriteLine("FOO is still not defined");
#endif
        }
    }
}
