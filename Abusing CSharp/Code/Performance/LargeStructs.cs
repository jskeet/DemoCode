using System;
using System.Diagnostics;

namespace Performance
{
    public struct Int256
    {
        private readonly long bits0;
        private readonly long bits1;
        private readonly long bits2;
        private readonly long bits3;

        public Int256(long bits0, long bits1, long bits2, long bits3)
        {
            this.bits0 = bits0;
            this.bits1 = bits1;
            this.bits2 = bits2;
            this.bits3 = bits3;
        }

        public long Bits0 { get { return bits0; } }
        public long Bits1 { get { return bits1; } }
        public long Bits2 { get { return bits2; } }
        public long Bits3 { get { return bits3; } }
    }

    class LargeStructs
    {
        private readonly Int256 value;

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

        static void Main()
        {
            new LargeStructs().RunTest();
        }
    }
}
