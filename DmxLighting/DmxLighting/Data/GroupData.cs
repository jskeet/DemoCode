// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Linq;

namespace DmxLighting.Data
{
    public class GroupData
    {
        public string Description { get; }

        public IReadOnlyList<ElementData> Elements { get; }

        public GroupData(string description, IEnumerable<ElementData> elements) =>
            (Description, Elements) = (description, elements.ToList().AsReadOnly());
    }
}
