﻿using System.Collections.Generic;

namespace VDrumExplorer.Models.Json
{
    internal sealed class InstrumentGroupJson
    {
        public string Description { get; set; }
        public Dictionary<int, string> Instruments { get; set; }

        public override string ToString() => Description;
    }
}
