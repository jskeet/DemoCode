// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Schema;
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

        // TODO: Property changes?
        public string Name
        {
            get => element.GetName(Universe[Channel]);
            set => Universe[Channel] = element.GetLowerValue(value);
        }
    }
}
