using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp7
{
    class ThrowExpression
    {
        static void Main()
        {
            string foo = null;
            string bar = CheckNotNull(foo, nameof(foo));
        }

        static T CheckNotNull<T>(T value, string name)
            => value == null ? value : throw new ArgumentNullException(name);
    }
}
