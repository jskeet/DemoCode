using System.Collections.Generic;

internal sealed class FreezableList<T> : List<T>
{
    // This would actually just implement IList<T>,
    // and prohibit changes after a Freeze call.

    internal void Freeze()
    {
    }
}
