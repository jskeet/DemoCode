using System;
using System.Diagnostics;

namespace Performance
{
    public struct Int256
    {
        public long Bits0 { get; }
        public long Bits1 { get; }
        public long Bits2 { get; }
        public long Bits3 { get; }

        public Int256(long bits0, long bits1, long bits2, long bits3)
        {
            Bits0 = bits0;
            Bits1 = bits1;
            Bits2 = bits2;
            Bits3 = bits3;
        }
    }

    class LargeStructs
    {
        private Int256 value;

        public LargeStructs()
        {
            value = new Int256(1L, 5L, 10L, 100L);
        }

        public long TotalValue
        {
            get
            {
                return value.Bits0 + value.Bits1 + value.Bits2 + value.Bits3;
            }
        }

        public void RunTest()
        {
            // Just make sure it’s JITted… 
            var sample = TotalValue;
            Stopwatch sw = Stopwatch.StartNew();
            long total = 0;
            for (int i = 0; i < 1000000000; i++)
            {
                total += TotalValue;
            }
            sw.Stop();
            Console.WriteLine("Total time: {0}ms", sw.ElapsedMilliseconds);
        }

        static void Main() => new LargeStructs().RunTest();
    }
}
