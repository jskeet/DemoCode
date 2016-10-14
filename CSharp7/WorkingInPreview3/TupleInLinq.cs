using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingInPreview3
{
    class TupleInLinq
    {
        static void Main()
        {
            // See http://stackoverflow.com/questions/39386251
            var list = new List<string> { "a", "b" };
            var dictionary = list.Select((value, index) => (value: value, index: index))
                                 .ToDictionary(pair => pair.index, pair => pair.value);
        }
    }
}
