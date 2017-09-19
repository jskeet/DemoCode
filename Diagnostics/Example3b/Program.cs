using NUnit.Common;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnitLite;
using System;
using System.Reflection;

namespace Example3b
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var writer = new ExtendedTextWrapper(Console.Out);
            return new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, writer, Console.In);
        }

        [Test]
        public void AreEqual()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var x = 10;
                var y = 10;
                Assert.AreEqual(x, y);
            }
        }

        [Test]
        public void ThatIsEqualTo()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var x = 10;
                var y = 10;
                Assert.AreEqual(x, y);
            }
        }

        [Test]
        public void ComparerConstruction()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var comparer = new NUnitEqualityComparer();
            }
        }

        [Test]
        public void ComparerAreEqual()
        {
            var comparer = new NUnitEqualityComparer();
            Tolerance tolerance = new Tolerance(0);
            for (int i = 0; i < 1000000; i++)
            {
                var x = 10;
                var y = 10;
                comparer.AreEqual(x, y, ref tolerance);
            }
        }

        [Test]
        public void IsEqualTo()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var y = 10;
                Is.EqualTo(y);
            }
        }
    }
}
