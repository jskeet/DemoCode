using System;

namespace Nullability
{
    class SneakyInParameter
    {
        static void Main()
        {
            string? text = "Not null";
            Action action = () => text = null;
            // Boom!
            int length = LengthIfPossible(in text, action);
        }

        static int LengthIfPossible(in string? text, Action action)
        {
            if (text == null)
            {
                return -1;
            }
            action();
            // We've checked whether text is null or not, right?
            // So if we're warning-free, all should be well...
            return text.Length;
        }
    }
}
