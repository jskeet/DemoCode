// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

using System.Collections.Generic;

internal sealed class FreezableList<T> : List<T>
{
    // This would actually just implement IList<T>,
    // and prohibit changes after a Freeze call.

    internal void Freeze()
    {
    }
}
