// Copyright 2017 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace StringInterpolation
{
    class LambdaExpressions
    {
        static void Main()
        {
            // Braces for multiple statements aren't allowed...
            // Console.WriteLine($"{{Console.WriteLine("hello"); return "y";}}");            

            // Lambda expressions don't have a type...
            // Console.WriteLine($"{() => "won't work"}");
            
            // ... but you can cast lambda expressions to specific
            // delegate types.
            Console.WriteLine($"{((Func<string>) (() => { Console.WriteLine("hello"); return "y"; })).Invoke()}");

            // Simplified with a method call to avoid the cast...
            Console.WriteLine($"{F(() => { Console.WriteLine("hello"); return "y"; })}");

            // Whole program in a string...
            Console.WriteLine($@"Hello {((Func<string>) (() =>
            {
                Console.Write("What's your name? ");
                return Console.ReadLine();
            }))()}!");
        }

        static string F(Func<string> func) => func();
    }
}
