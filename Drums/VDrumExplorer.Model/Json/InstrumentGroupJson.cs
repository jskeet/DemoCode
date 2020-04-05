﻿// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;

namespace VDrumExplorer.Model.Json
{
    using static Validation;

    internal sealed class InstrumentGroupJson
    {
        public string? Description { get; set; }
        public Dictionary<int, string>? Instruments { get; set; }
        public Dictionary<string, Dictionary<int, int>>? VeditDefaults { get; set; }

        public override string ToString() => Description ?? "(Unspecified)";

        internal InstrumentGroup ToInstrumentGroup(int index) =>
            new InstrumentGroup(
                ValidateNotNull(Description, nameof(Description)),
                index,
                ValidateNotNull(Instruments, nameof(Instruments)),
                VeditDefaults);
    }
}
