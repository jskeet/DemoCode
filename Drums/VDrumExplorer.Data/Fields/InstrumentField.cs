// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VDrumExplorer.Data.Fields
{
    public class InstrumentField : PrimitiveFieldBase, IPrimitiveField
    {
        /// <summary>
        /// The offset within the parent container that determines whether the instrument is a preset
        /// instrument or a user sample.
        /// </summary>
        private readonly int bankOffset;

        /// <summary>
        /// The offset of the vedit container relative to the parent container.
        /// </summary>
        public int VeditOffset { get; }

        internal InstrumentField(Parameters common, int bankOffset, int veditOffset) : base(common) =>
            (this.bankOffset, VeditOffset) = (bankOffset, veditOffset);

        public override string GetText(FixedContainer context, ModuleData data)
        {
            var instrument = GetInstrument(context, data);
            return instrument.ToString();
        }

        public Instrument GetInstrument(FixedContainer context, ModuleData data)
        {
            var instrumentAddress = GetAddress(context);
            var bankAddress = context.Address + bankOffset;
            // TODO: Maybe put this code into ModuleData to avoid the duplication.
            int rawValue = (short) (
                    (data.GetAddressValue(instrumentAddress) << 12) |
                    (data.GetAddressValue(instrumentAddress + 1) << 8) |
                    (data.GetAddressValue(instrumentAddress + 2) << 4) |
                    data.GetAddressValue(instrumentAddress + 3));
            byte instrumentBank = data.GetAddressValue(bankAddress);            
            return instrumentBank switch
            {
                0 => Schema.PresetInstruments[rawValue],
                1 => Schema.UserSampleInstruments[rawValue],
                _ => throw new InvalidOperationException($"Invalid instrument bank {instrumentBank}")
            };
        }

        public override bool TrySetText(FixedContainer context, ModuleData data, string text)
        {
            var instrument = Schema.PresetInstruments.Concat(Schema.UserSampleInstruments).FirstOrDefault(inst => inst.Name == text);
            if (instrument != null)
            {
                SetInstrument(context, data, instrument);
                return false;
            }
            return true;
        }

        public void SetInstrument(FixedContainer context, ModuleData data, Instrument instrument)
        {
            var instrumentAddress = GetAddress(context);
            var bankAddress = context.Address + bankOffset;
            byte[] idBytes =
            {
                (byte) ((instrument.Id >> 12) & 0xf),
                (byte) ((instrument.Id >> 8) & 0xf),
                (byte) ((instrument.Id >> 4) & 0xf),
                (byte) ((instrument.Id >> 0) & 0xf)
            };
            // Set everything to do with the instrument itself in one set operation.
            byte[] bankBytes = new[] { instrument.Group != null ? (byte) 0 : (byte) 1 };
            data.SetMultipleData((instrumentAddress, idBytes), (bankAddress, bankBytes));

            // Then set any appropriate vedit information separately.
            // (We may want to do this all in one go, but it doesn't matter too much.)
            var veditAddress = context.Address + VeditOffset;
            var veditContainer = Schema.LoadableContainersByAddress[veditAddress];
            var veditContext = new FixedContainer(veditContainer, veditAddress);
            var defaultFieldValues = instrument?.DefaultFieldValues;
            
            // Reset each field in the instrument's vedit based on either the schema default, or the instrument-specific value.
            foreach (var field in veditContext.GetChildren(data).OfType<IPrimitiveField>())
            {
                if (field is NumericField numeric && defaultFieldValues != null && defaultFieldValues.TryGetValue(field.Name, out int value))
                {
                    numeric.SetRawValue(veditContext, data, value);
                }
                else
                {
                    field.Reset(veditContext, data);
                }
            }
        }

        public override void Reset(FixedContainer context, ModuleData data) => SetInstrument(context, data, Schema.PresetInstruments.First());

        protected override bool ValidateData(FixedContainer context, ModuleData data, [NotNullWhen(false)] out string? error)
        {
            var instrumentAddress = GetAddress(context);
            var bankAddress = context.Address + bankOffset;
            int rawValue = (short) (
               (data.GetAddressValue(instrumentAddress) << 12) |
               (data.GetAddressValue(instrumentAddress + 1) << 8) |
               (data.GetAddressValue(instrumentAddress + 2) << 4) |
               data.GetAddressValue(instrumentAddress + 3));
            byte instrumentBank = data.GetAddressValue(bankAddress);
            if (instrumentBank > 1)
            {
                error = $"Invalid instrument bank {instrumentBank}";
                return false;
            }
            int maxExclusive = instrumentBank == 0 ? Schema.PresetInstruments.Count : Schema.UserSampleInstruments.Count;
            if (rawValue < 0 || rawValue >= maxExclusive)
            {
                error = $"Invalid instrument ID {rawValue}";
                return false;
            }

            error = null;
            return true;
        }
    }
}
