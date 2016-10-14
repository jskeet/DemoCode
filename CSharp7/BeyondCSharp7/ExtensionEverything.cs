using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondCSharp7
{
    public class NoPublicConstructors
    {
        private NoPublicConstructors()
        {
        }

        public static NoPublicConstructors Create() => new NoPublicConstructors();
    }

    public static class Extensions
    {
        // Making up syntax here...
        public static NoPublicConstructors NoPublicConstructors.new()
        {
            return NoPublicConstructors.Create();
        }

        // And here
        public static 
    }

    class ExtensionEverything
    {
        static void Main()
        {
            var x = new NoPublicConstructors();
        }
    }
}
