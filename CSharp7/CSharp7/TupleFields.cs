using System;

namespace CSharp7
{
    class TupleFields
    {
        private (int x, int y) counters;

        static void Main()
        {
            var fields = new TupleFields();
            fields.IncrementBoth();
            fields.IncrementX();
            Console.WriteLine(fields.counters);
        }

        void IncrementBoth()
        {
            counters.x++;
            counters.y++;
        }

        void IncrementX()
        {
            counters.x++;
        }

        void IncrementY()
        {
            counters.x++;
        }
    }
}
