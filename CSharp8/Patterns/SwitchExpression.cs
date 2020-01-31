using System;
using System.Collections.Generic;

namespace PatternsAndRanges
{
    class SwitchExpression
    {
        static void Main()
        {
            Func<int, int> fib = Fib4a;
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{i}: {fib(i)}");
            }
        }

        static int Fib1(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (n == 0)
            {
                return 0;
            }
            if (n == 1)
            {
                return 1;
            }
            return Fib1(n - 1) + Fib1(n - 2);
        }

        static int Fib2(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            switch (n)
            {
                case 0: return 0;
                case 1: return 1;
                default: return Fib2(n - 1) + Fib2(n - 2);
            }
        }

        static int Fib3(int n)
        {
            switch (n)
            {
                case 0: return 0;
                case 1: return 1;
                default: return n >= 0 ? Fib3(n - 1) + Fib3(n - 2)
                        : throw new ArgumentOutOfRangeException();
            }
        }

        static int Fib4a(int n)
        {
            int result = n switch
            {
                0 => 0,
                1 => 1,
                _ => n >= 0 ? Fib4a(n - 1) + Fib4a(n - 2)
                  : throw new ArgumentOutOfRangeException()
            };
            return result;
        }

        static int Fib4b(int n) => n switch
        {
            0 => 0,
            1 => 1,
            _ => n >= 0 ? Fib4b(n - 1) + Fib4b(n - 2)
              : throw new ArgumentOutOfRangeException()
        };

        static int Fib5(int n) => n switch
        {
            _ when n < 0 => throw new ArgumentOutOfRangeException(),
            0 => 0,
            1 => 1,
            _ => Fib5(n - 1) + Fib5(n - 2)
        };

        static int Fib6(int n)
        {
            return Impl((0, 1), 0);

            int Impl((int current, int next) tuple, int index) =>
                index == n ? tuple.current : Impl((tuple.next, tuple.current + tuple.next), index + 1);
        }

        // Just for completeness... the *nice* version.
        // (No switch expressions though...)
        static IEnumerable<int> Fib7()
        {
            int current = 0;
            int next = 1;
            while (true)
            {
                yield return current;
                (current, next) = (next, current + next);
            }
        }
    }
}
