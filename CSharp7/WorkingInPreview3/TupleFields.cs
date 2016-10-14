using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
{
    class TupleFields
    {
        private (int x, int y) counters;

        private string name;

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
