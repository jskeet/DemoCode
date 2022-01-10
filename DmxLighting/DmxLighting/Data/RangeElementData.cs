// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Schema;

namespace DmxLighting.Data
{
    public class RangeElementData : ElementData
    {
        private readonly RangeElement element;

        public RangeElementData(DmxUniverse universe, int channel, RangeElement element) : base(universe, channel, element.Description)
        {
            this.element = element;
        }

        public byte Value
        {
            get => RawValue;
            set => RawValue = value;
        }
    }
}
