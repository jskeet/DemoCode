// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    public sealed class DynamicOverlay : FieldBase
    {
        public int SwitchContainerOffset { get; }

        private readonly string switchField;
        private IReadOnlyList<Container> OverlaidContainers { get; }

        internal DynamicOverlay(FieldBase.Parameters common, int switchContainerOffset, string switchField, IReadOnlyList<Container> containers)
            : base(common)
        {
            (SwitchContainerOffset, this.switchField, OverlaidContainers) = (switchContainerOffset, switchField, containers);
        }

        public Container GetOverlaidContainer(FixedContainer context, ModuleData data)
        {
            var switchContainerAddress = context.Address + SwitchContainerOffset;
            var switchContainer = Schema.LoadableContainersByAddress[switchContainerAddress];
            var switchContext = new FixedContainer(switchContainer, switchContainerAddress);
            var field = switchContainer.GetField(switchField);

            int index = field switch
            {
                // User samples get an extra overlay at the end.
                InstrumentField instrumentField =>
                    instrumentField.GetInstrument(switchContext, data).Group?.Index ?? Schema.InstrumentGroups.Count,
                NumericFieldBase nfb => nfb.GetRawValue(switchContext, data),
                _ => throw new InvalidOperationException($"Invalid switch field type '{field.GetType()}'")
            };
            return OverlaidContainers[index];
        }
    }
}
