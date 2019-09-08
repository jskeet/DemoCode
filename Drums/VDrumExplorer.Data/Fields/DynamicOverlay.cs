// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;

namespace VDrumExplorer.Data.Fields
{
    public sealed class DynamicOverlay : FieldBase
    {
        private InstrumentField instrumentField;

        public int SwitchOffset { get; }
        
        private readonly string? switchTransform;
        internal IReadOnlyList<Container> OverlaidContainers { get; }

        // No condition; overlays are already conditional effectively.
        internal DynamicOverlay(FieldBase.Parameters common, int switchOffset, string? switchTransform, IReadOnlyList<Container> containers)
            : base(common)
        {
            (SwitchOffset, this.switchTransform, OverlaidContainers) = (switchOffset, switchTransform, containers);
            // FIXME: This is really horribly, but it works for now.
            instrumentField = new InstrumentField(new FieldBase.Parameters(Schema, "dummy instrument field", 0, 4, "dummy instrument field", null), 8);
        }

        public Container GetOverlaidContainer(FixedContainer context, ModuleData data)
        {
            // FIXME: This is really hacky at the moment...
            var switchAddress = context.Address + SwitchOffset;
            int index = switchTransform switch
            {
                null => data.GetAddressValue(switchAddress),
                "instrumentGroup" => GetInstrumentGroupIndex(),
                _ => throw new InvalidOperationException($"Invalid switch transform '{switchTransform}'")
            };
            return OverlaidContainers[index];
            
            int GetInstrumentGroupIndex()
            {
                // This is in the wrong container. It's all horrible!
                var instrumentContext = new FixedContainer(context.Container, switchAddress);
                var instrument = instrumentField.GetInstrument(instrumentContext, data);
                // User samples get an extra overlay at the end.
                return instrument.Group?.Index ?? Schema.InstrumentGroups.Count;
            }
        }
    }
}
