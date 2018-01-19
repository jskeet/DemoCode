// Copyright 2018 Jon Skeet. All rights reserved. Use of this source code is governed by the Apache License 2.0, as found in the LICENSE.txt file.
using System;

namespace CSharp7
{
    class LocalFunctions3
    {
        static void Main()
        {
            NonCapture();
            CaptureEfficiently();
            CaptureInefficiently();
        }

        static void NonCapture()
        {
            int i = 0;

            PrintValue(i);

            void PrintValue(int value)
            {
                Console.WriteLine(value);
            }
        }

        static void CaptureEfficiently()
        {
            int i = 0;
            PrintValue();

            void PrintValue()
            {
                Console.WriteLine(i);
            }
        }

        static void CaptureInefficiently()
        {
            int i = 0;
            Action action = PrintValue;

            void PrintValue()
            {
                Console.WriteLine(i);
            }
        }
    }
}
