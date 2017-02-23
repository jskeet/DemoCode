// Contributed by Jeroen Bos.

using System;
using System.Collections.Generic;

namespace OddsAndEnds
{
    class EmptyEnumerables
    {
        static void Main()
        {
            DisplayIfEmpty(new string[0]);
            DisplayIfEmpty(new string[] { "foo", "bar", "baz" });
        }

        static void DisplayIfEmpty(IEnumerable<string> words)
        {
            if (IsEmpty(ref words))
            {
                Console.WriteLine("The sequence was empty!");
            }
            else
            {
                Console.WriteLine("The sequence was not empty:");
                foreach (var word in words)
                {
                    Console.WriteLine($"  {word}");
                }
            }
        }

        public static bool IsEmpty<T>(ref IEnumerable<T> sequence)
        {
            var enumerator = sequence.GetEnumerator();
            bool isEmpty = !enumerator.MoveNext();
            sequence = EnumerateOverTheRest();
            return isEmpty;

            IEnumerable<T> EnumerateOverTheRest()
            {
                if (!isEmpty)
                {
                    do
                    {
                        yield return enumerator.Current;
                    } while (enumerator.MoveNext());
                }
                enumerator.Dispose();
            }
        }
    }
}
