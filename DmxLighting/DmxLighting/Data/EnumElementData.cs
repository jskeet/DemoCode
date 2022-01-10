// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Schema;
using DmxLighting.Utility;
using System.Collections.Generic;

namespace DmxLighting.Data
{
    public class EnumElementData : ElementData
    {
        public IReadOnlyList<string> AllNames => element.Names;

        private readonly EnumElement element;

        public EnumElementData(DmxUniverse universe, int channel, EnumElement element) : base(universe, channel, element.Description)
        {
            this.element = element;
        }

        [RelatedProperties(nameof(Value), nameof(LowerBoundInclusive), nameof(UpperBoundInclusive))]
        public string Name
        {
            get => element.GetRange(RawValue).Name;
            set => SetProperty(Name, value, x => RawValue = element.GetLowerValue(x));
        }

        public byte LowerBoundInclusive => element.GetRange(RawValue).LowerBoundInclusive;
        public byte UpperBoundInclusive => element.GetRange(RawValue).UpperBoundInclusive;

        public byte Value
        {
            get => RawValue;
            set => RawValue = value;
        }
    }
}
