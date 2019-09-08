// Copyright 2019 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
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

        internal InstrumentField(Parameters common, int bankOffset) : base(common) =>
            this.bankOffset = bankOffset;

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
            byte[] bankBytes = new[] { instrument.Group != null ? (byte) 0 : (byte) 1 };
            data.SetMultipleData((instrumentAddress, idBytes), (bankAddress, bankBytes));
        }

        public override void Reset(FixedContainer context, ModuleData data) => SetInstrument(context, data, Schema.PresetInstruments.First());

        protected override bool ValidateData(FixedContainer context, ModuleData data, out string? error)
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
