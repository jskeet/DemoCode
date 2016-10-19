// Copyright 2016 Jon Skeet. All Rights Reserved.
// Licensed under the Apache License Version 2.0.

namespace Slides
{
    /// <summary>
    /// This is immutable. Really. I promise that no maintenance
    /// programmer ever will add a method that calls the setter,
    /// because they will all read this comment.
    /// </summary>
    public sealed class LazilyImmutable
    {
        public int Value { get; private set; }

        public void FixBug()
        {
            // I didn't bother reading the comment. This should be fine.
            Value = 20;
        }

        public LazilyImmutable(int value)
        {
            Value = value;
        }
    }
}
