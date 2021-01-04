using System;
using System.RuntimeCompilerServices;

namespace CameraControl.Visca
{
    internal static class Preconditions
    {
        internal static T CheckNotNull<T>(T value, [CallerArgumentExpression("value")] string paramName = "") =>
            value ?? throw new ArgumentNullException(paramName);

        internal static void CheckRange(int value, int minInclusive, int maxInclusive,
            [CallerArgumentExpression("value")] string paramName = "")
        {
            if (value < minInclusive || value > maxInclusive)
            {
                throw new ArgumentOutOfRangeException(paramName, $"Value must be in the range [{minInclusive}-{maxInclusive}]");
            }
        }
    }
}
