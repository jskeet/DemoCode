// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Utility;

namespace DmxLighting.Data
{
    public abstract class ElementData : ViewModelBase
    {
        protected DmxUniverse Universe { get; }
        public int Channel { get; }
        public string Description { get; }

        public ElementData(DmxUniverse universe, int channel, string description)
        {
            Universe = universe;
            Channel = channel;
            Description = description;
        }
    }
}
