using System;
using System.IO;

namespace MinorFeatures
{
    class EnhancedUsing
    {
        static void Main()
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[10].AsSpan());

            using var duck = new DuckDisposable();
        }

        // Note: "structural using" only works with ref struct types,
        // but the error message doesn't mention that.
        // See https://github.com/dotnet/roslyn/issues/33746
        public ref struct DuckDisposable
        {
            public void Dispose()
            {
                Console.WriteLine("Disposed");
            }
        }
    }
}
